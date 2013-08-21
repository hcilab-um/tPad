from itertools import product

x = []
y = []
z = []
fileExt = ".csv"
direction = ["FaceUp", "FaceDown"]
orientation = ["Portrait", "RightLandscape", "InversePortrait", "LeftLandscape"]
fileNames = []

for i in product(direction, orientation):
	fileName = "".join(i) + fileExt
	fileNames.append(fileName)

#encode the files
for i, fileName in enumerate(fileNames):
	f = open(fileName)
	w = open("encoded" + fileName, "w")
	for line in f:
		w.write(line.strip())
		w.write(",")
		w.write(",".join(["0"]*i))
		if i != 0:
			w.write(",")
		w.write("1")
		if i != len(fileNames) - 1:
			w.write(",")
		w.write(",".join(["0"]*(len(fileNames) - i - 1)))
		w.write("\n")

	f.close()
	w.close()


#consolidate the files
w = open("consolidated.csv", "w")
for i, fileName in enumerate(fileNames):
	f = open("encoded" + fileName)
	for line in f:
		w.write(line)

	f.close()

w.close()

#normalize the x,y and z data
