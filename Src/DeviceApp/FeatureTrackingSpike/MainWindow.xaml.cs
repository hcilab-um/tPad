using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
//using System.Windows.Shapes;
using System.Drawing;

using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.UI;
using Emgu.CV.Util;

namespace FeatureTrackingSpike
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    private VectorOfKeyPoint imageKeyPoints;
    private Matrix<float> pageImageDescriptors;
    private SURFDetector surf = new SURFDetector(500, false);
    //private SIFTDetector sift = new SIFTDetector();
    private Emgu.CV.Flann.Index flannIdx;
    private int minSize = 20;
    private int maxSize = 150;
    private float radius_red = 20;

    public MainWindow()
    {
      InitializeComponent();

      //load page image(s)
      Image<Gray, Byte> pageImage = new Image<Gray, Byte>("paper_page.png");
      //pageImage = pageImage.Resize(0.7, INTER.CV_INTER_LINEAR);

      //extract features from the page image and build tree
      VectorOfKeyPoint imagePageKeyPoints = surf.DetectKeyPointsRaw(pageImage, null);
      imagePageKeyPoints.FilterByKeypointSize(minSize, maxSize);
      
      //reduce keyPoints      
      imageKeyPoints = reduceKeyPoints(ref imagePageKeyPoints, ref radius_red);
      //end test

      pageImageDescriptors = surf.ComputeDescriptorsRaw(pageImage, null, imageKeyPoints);
      flannIdx = new Emgu.CV.Flann.Index(pageImageDescriptors, 6);      

      long initTimeWithImageLoad = DateTime.Now.Ticks;

      //load device image
      Image<Gray, Byte> img = new Image<Gray, Byte>("photo.JPG");
      img = img.Resize(0.28, INTER.CV_INTER_LINEAR);
            
      long initTimeSimple = DateTime.Now.Ticks;
      long initTimeWarp = DateTime.Now.Ticks;
      //rectify device image
      Image<Gray, Byte> deviceImage = new Image<Gray, Byte>(img.Width, img.Height);
      HomographyMatrix homography_warp = imageWarp(ref img, ref deviceImage);
      long endTimeWarp = DateTime.Now.Ticks;
      TimeSpan lenghtWarp = new TimeSpan(endTimeWarp - initTimeWarp);
 
      //find SURF features
      HomographyMatrix locationHM = computeSURF(ref deviceImage, ref pageImage, ref homography_warp);

      long endTime = DateTime.Now.Ticks;
      TimeSpan lenghtSimple = new TimeSpan(endTime - initTimeSimple);
      TimeSpan lenghtWithImageLoad = new TimeSpan(endTime - initTimeWithImageLoad);   
    }

    private VectorOfKeyPoint reduceKeyPoints(ref VectorOfKeyPoint imagePoints, ref float radius)
    {
      MKeyPoint[] keyPointsArray = imagePoints.ToArray();
      MKeyPoint[] reduced_KeyPointsArray = new MKeyPoint[keyPointsArray.Length];

      int[] indices = new int[keyPointsArray.Length];
      bool[] processed = new bool[keyPointsArray.Length]; //false by default
      int j = 0;
      for (int i = 0; i < keyPointsArray.Length; i++)
      {
        if (processed[i] == true)
        {
          continue;
        }

        processed[i] = true;

        for (int k = (i + 1); k < keyPointsArray.Length; k++)
        {
          if (processed[k] == true)
          {
            continue;
          }

          double eucl_dist = Math.Sqrt(((keyPointsArray[i].Point.X - keyPointsArray[k].Point.X) * (keyPointsArray[i].Point.X - keyPointsArray[k].Point.X)) +
            ((keyPointsArray[i].Point.Y - keyPointsArray[k].Point.Y) * (keyPointsArray[i].Point.Y - keyPointsArray[k].Point.Y)));

          if (eucl_dist < radius)
          {
            processed[k] = true;
          }
        }

        reduced_KeyPointsArray[j] = keyPointsArray[i];
        j++;
      }

      MKeyPoint[] reduced_KeyPointsArray2 = new MKeyPoint[j];
      for (int i = 0; i < j; i++)
        reduced_KeyPointsArray2[i] = reduced_KeyPointsArray[i];
      imagePoints.Clear();
      imagePoints.Push(reduced_KeyPointsArray2);

      return imagePoints;
    }

    private HomographyMatrix computeSURF(ref Image<Gray, Byte> deviceImage, ref Image<Gray, Byte> pageImage, ref HomographyMatrix warpMat)
    {
      HomographyMatrix locationMat = null;
      Matrix<byte> mask;
      
      //extract features from the device image
      VectorOfKeyPoint deviceKeyPoints = surf.DetectKeyPointsRaw(deviceImage, null);      
      deviceKeyPoints.FilterByKeypointSize(minSize, maxSize);      
      VectorOfKeyPoint deviceImageKeyPoints = reduceKeyPoints(ref deviceKeyPoints, ref radius_red);

      long init = DateTime.Now.Ticks;      
      Matrix<float> modelDescriptors = surf.ComputeDescriptorsRaw(deviceImage, null, deviceImageKeyPoints);      //compute descriptors for keypoints //0.03 sec
      long end = DateTime.Now.Ticks;
      TimeSpan lenghtMatching = new TimeSpan(end - init);
      
      //match features
      //For each descriptor in the first set, this matcher finds the closest descriptor in the second set by trying each one      
      //Matrix<int> indices;
      //Matrix<float> dist;
      //BruteForceMatcher matcher = new BruteForceMatcher(BruteForceMatcher.DistanceType.L2F32);
      //matcher.Add(modelDescriptors);
      //int k = 2;
      //indices = new Matrix<int>(pageImageDescriptors.Rows, k);
      //dist = new Matrix<float>(pageImageDescriptors.Rows, k);
      //matcher.KnnMatch(pageImageDescriptors, indices, dist, k, null);

      //DEFAULT FLANN/ Linear FLANN: When passing an object of this type, the index will perform a linear, brute-force search (openCV)
      //KMean, multiple KDTree, Composite (cmobi of KDTree and KMean), Autotuned:
      //Emgu.CV.Flann.Index flannIdx = new Emgu.CV.Flann.Index(modelDescriptors, 10, 10, Emgu.CV.Flann.CenterInitType.RANDOM, 0.2f); //test flann matcher      
      int flannCheck = 15;
      int flannK = 2;
      Matrix<int> flannIndices = new Matrix<int>(modelDescriptors.Rows, flannK);
      Matrix<float> flannDist = new Matrix<float>(modelDescriptors.Rows, flannK);
      flannIdx.KnnSearch(modelDescriptors, flannIndices, flannDist, flannK, flannCheck);                
      

      //mask = new Matrix<byte>(dist.Rows, 1);
      //mask.SetValue(255);
      ////filter matched features (rejects matches which are not unique)
      //Features2DTracker.VoteForUniqueness(dist, 0.8, mask);
      //int nonZeroCount = CvInvoke.cvCountNonZero(mask);
      //if (nonZeroCount >= 4)
      //{
      //  nonZeroCount = Features2DTracker.VoteForSizeAndOrientation(deviceImageKeyPoints, pageImageKeyPoints, indices, mask, 1.5, 20);
      //  if (nonZeroCount >= 4)
      //    homography = Features2DTracker.GetHomographyMatrixFromMatchedFeatures(deviceImageKeyPoints, pageImageKeyPoints, indices, mask, 3);
      //}

      long timeVote = DateTime.Now.Ticks;   
      mask = new Matrix<byte>(flannDist.Rows, 1);
      mask.SetValue(255);
      //filter matched features (rejects matches which are not unique)
      Features2DTracker.VoteForUniqueness(flannDist, 0.8, mask);
      int nonZeroCount = CvInvoke.cvCountNonZero(mask);
      if (nonZeroCount >= 4)
      {
        nonZeroCount = Features2DTracker.VoteForSizeAndOrientation(imageKeyPoints, deviceImageKeyPoints, flannIndices, mask, 1.1, 5);
        if (nonZeroCount >= 4)
          locationMat = Features2DTracker.GetHomographyMatrixFromMatchedFeatures(imageKeyPoints, deviceImageKeyPoints, flannIndices, mask, 5);
      }
      long endVote = DateTime.Now.Ticks;
      lenghtMatching = new TimeSpan(endVote - timeVote); //0.04sec

      //Draw the matched keypoints
      Image<Bgr, Byte> result = Features2DTracker.DrawMatches(deviceImage, deviceImageKeyPoints, pageImage, imageKeyPoints,
         flannIndices, new Bgr(255, 0, 0), new Bgr(255, 0, 0), mask, Features2DTracker.KeypointDrawType.DEFAULT);

      if (locationMat != null)
      {  //draw a rectangle along the projected model
        Rectangle rect = deviceImage.ROI;
        PointF[] pts = new PointF[] {new PointF(rect.Left, rect.Bottom), new PointF(rect.Right, rect.Bottom),
          new PointF(rect.Right, rect.Top), new PointF(rect.Left, rect.Top)};
        warpMat.ProjectPoints(pts);
        locationMat.ProjectPoints(pts);

        result.DrawPolyline(Array.ConvertAll<PointF, System.Drawing.Point>(pts, System.Drawing.Point.Round), true, new Bgr(System.Drawing.Color.Red), 5);
      }

      ImageViewer.Show(result, "matching");
      CvInvoke.cvDestroyWindow("matching");

      return locationMat;
    }

    private HomographyMatrix imageWarp(ref Image<Gray, Byte> src, ref Image<Gray, Byte> dest)
    {            
      HomographyMatrix homography = new HomographyMatrix();

      IntPtr point = CvInvoke.cvCreateMat(2, 4, MAT_DEPTH.CV_32F);
      CvInvoke.cvSet2D(point, 0, 0, new MCvScalar(9));
      CvInvoke.cvSet2D(point, 1, 0, new MCvScalar(72));
      CvInvoke.cvSet2D(point, 0, 1, new MCvScalar(491));
      CvInvoke.cvSet2D(point, 1, 1, new MCvScalar(240));
      CvInvoke.cvSet2D(point, 0, 2, new MCvScalar(304));
      CvInvoke.cvSet2D(point, 1, 2, new MCvScalar(590));
      CvInvoke.cvSet2D(point, 0, 3, new MCvScalar(34));
      CvInvoke.cvSet2D(point, 1, 3, new MCvScalar(543));

      IntPtr point_org = CvInvoke.cvCreateMat(2, 4, MAT_DEPTH.CV_32F);
      CvInvoke.cvSet2D(point_org, 0, 0, new MCvScalar(96));
      CvInvoke.cvSet2D(point_org, 1, 0, new MCvScalar(307));
      CvInvoke.cvSet2D(point_org, 0, 1, new MCvScalar(386));
      CvInvoke.cvSet2D(point_org, 1, 1, new MCvScalar(306));
      CvInvoke.cvSet2D(point_org, 0, 2, new MCvScalar(386));
      CvInvoke.cvSet2D(point_org, 1, 2, new MCvScalar(664));
      CvInvoke.cvSet2D(point_org, 0, 3, new MCvScalar(163));
      CvInvoke.cvSet2D(point_org, 1, 3, new MCvScalar(681));

      IntPtr mask = CvInvoke.cvCreateMat(3, 1, MAT_DEPTH.CV_32F);

      CvInvoke.cvFindHomography(point, point_org, homography, HOMOGRAPHY_METHOD.DEFAULT, 0, new IntPtr());
      CvInvoke.cvWarpPerspective(src, dest, homography, (int)WARP.CV_WARP_DEFAULT, new MCvScalar(0));
      
      return homography;
    }

    //private void SetBumper(IImage ocvBumper, System.Windows.Controls.Image iTargetUI)
    //{
    //  if (ocvBumper == null)
    //    return;

    //  IntPtr ptrCB = ocvBumper.Bitmap.GetHbitmap();
    //  BitmapSource bsCB = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ptrCB,
    //    IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
    //  DeleteObject(ptrCB);
    //  iTargetUI.Source = bsCB;
    //}

  }
}
