using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TallComponents.PDF;
using System.Windows;
using TallComponents.PDF.TextExtraction;

namespace UofM.HCI.tPab.App.ActiveReader
{
  public class PDFContentHelper
  {

    public String DocumentPath { get; set; }

    public PDFContentHelper(String docPath)
    {
      DocumentPath = docPath;
    }
       
    public string PixelToContent(Point position, int actualPage, double width, double height, out Rect wordBounds)
    {
      using (FileStream fileIn = new FileStream(DocumentPath, FileMode.Open, FileAccess.Read))
      {
        //0- Open and load the PDF
        Document PdfDocument = new Document(fileIn);
        
        //1- try to find the piece of content the mouse is hovering
        TallComponents.PDF.Page page = PdfDocument.Pages[actualPage];

        double widthT = width / page.Width;
        double heightT = height / page.Height;

        //retrieve all glyphs from the current page
        //Notice that you grep a strong reference to the glyphs, otherwise the GC can decide to recycle. 
        GlyphCollection glyphs = page.Glyphs;

        //default the glyph collection is ordered as they are present in the PDF file.
        //we want them in reading order.
        glyphs.Sort();

        //the bounds of the last glyph analysed
        Rect glyphBounds = Rect.Empty;

        //the current word over which the user clicked
        StringBuilder currentWord = new StringBuilder();
        wordBounds = Rect.Empty;
        bool foundWord = false;
        
        foreach (Glyph glyph in glyphs)
        {
          if (glyph.Characters.Length == 0 || glyph.Characters[0] == ' ')
          {
            if (foundWord)
            {
              double wordWidth = glyphBounds.Right - wordBounds.Left;
              if (wordWidth > 0) //multi-line word -- the bounds cover only the upper part of it
                wordBounds = new Rect(wordBounds.Left, wordBounds.Top, wordWidth, wordBounds.Height);

              return currentWord.ToString();
            }

            wordBounds = Rect.Empty;
            currentWord.Clear();
            continue;
          }         
          
          glyphBounds = new Rect(
            glyph.TopLeft.X,
            page.Height - glyph.TopLeft.Y,
            glyph.TopRight.X - glyph.TopLeft.X,
            glyph.TopLeft.Y - glyph.BottomLeft.Y);
          glyphBounds.Scale(widthT, heightT);

          if (wordBounds == Rect.Empty)
            wordBounds = glyphBounds;

          string chars = String.Empty;
          foreach (char ch in glyph.Characters)
            currentWord.Append(ch);

          if (!glyphBounds.Contains(position))
            continue;

          foundWord = true;
          //Console.WriteLine("{0} -[{1},{2},{3},{4}] Font={5}({6})", chars, glyph.BottomLeft,
          //  glyph.BottomRight, glyph.TopLeft, glyph.TopRight, glyph.Font.Name, glyph.FontSize);
        }
        return null;
      }
    }

    public List<ContentLocation> ContentToPixel(String wordToSearch, int actualPage, double width, double height)
    {
      List<ContentLocation> results = new List<ContentLocation>();
      if (wordToSearch == null || wordToSearch.Length == 0)
        return results;

      using (FileStream fileIn = new FileStream(DocumentPath, FileMode.Open, FileAccess.Read))
      {
        //0- Open and load the PDF
        Document PdfDocument = new Document(fileIn);

        for (int pageIndex = 0; pageIndex < PdfDocument.Pages.Count; pageIndex++)
        {
          // searches only on the actual page, or on all of them if actualPage == -1
          if (actualPage != -1 && actualPage != pageIndex)
            continue;

          //1- try to find the piece of content the mouse is hovering
          TallComponents.PDF.Page page = PdfDocument.Pages[pageIndex];

          double widthT = width / page.Width;
          double heightT = height / page.Height;

          //retrieve all glyphs from the current page
          //Notice that you grep a strong reference to the glyphs, otherwise the GC can decide to recycle. 
          GlyphCollection glyphs = page.Glyphs;

          //default the glyph collection is ordered as they are present in the PDF file.
          //we want them in reading order.
          glyphs.Sort();

          //the bounds of the last glyph analysed
          Rect glyphBounds = Rect.Empty;

          //the current word over which the user clicked
          StringBuilder currentWord = new StringBuilder();
          Rect wordBounds = Rect.Empty;
          bool foundWord = false;
          int wordIndex = 0;

          foreach (Glyph glyph in glyphs)
          {
            if (glyph.Characters.Length == 0 || wordIndex == 0)
            {
              if (foundWord)
              {
                double wordWidth = glyphBounds.Right - wordBounds.Left;
                if (wordWidth > 0) //multi-line word -- the bounds cover only the upper part of it
                  wordBounds = new Rect(wordBounds.Left, wordBounds.Top, wordWidth, wordBounds.Height);

                foundWord = false;
                results.Add(new ContentLocation() { Content = currentWord.ToString(), PageIndex = pageIndex, ContentBounds = wordBounds });           
              }

              wordIndex = 0;
              wordBounds = Rect.Empty;
              currentWord.Clear();
              //continue;
            }

            glyphBounds = new Rect(
              glyph.TopLeft.X,
              page.Height - glyph.TopLeft.Y,
              glyph.TopRight.X - glyph.TopLeft.X,
              glyph.TopLeft.Y - glyph.BottomLeft.Y);
            glyphBounds.Scale(widthT, heightT);

            if (wordBounds == Rect.Empty)
              wordBounds = glyphBounds;

            string chars = String.Empty;
            foreach (char ch in glyph.Characters)
              currentWord.Append(ch);
            
            if (!wordToSearch[wordIndex].ToString().Equals(glyph.Characters[0].ToString(), StringComparison.CurrentCultureIgnoreCase))
              wordIndex = 0;             
            else wordIndex++;

            if (wordIndex == wordToSearch.Length)
            {
              foundWord = true;
              wordIndex = 0;             
            }
          }
        }
      }

      return results;
    }
  }
}
