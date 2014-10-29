from itertools import product

values = set()
fileExt = ".csv"
direction = ["FaceUp", "FaceDown"]
orientation = ["Portrait", "RightLandscape", "InversePortrait", "LeftLandscape"]
fileNames = []
encodedPrefix = "encoded"

for i in product(direction, orientation):
	fileName = "".join(i) + fileExt
	fileNames.append(fileName)

#encode the files
for i, fileName in enumerate(fileNames):
	f = open(fileName)
	w = open(encodedPrefix + fileName, "w")
	encoding = ["0"] * len(fileNames)
	encoding[i] = "1"
	for line in f:
		w.write(line.strip())
		w.write(",")
		w.write(",".join(encoding))
		w.write("\n")

	f.close()
	w.close()


#consolidate the files
w = open("consolidated.csv", "w")
for i, fileName in enumerate(fileNames):
	f = open(encodedPrefix + fileName)
	for line in f:
		w.write(line)

	f.close()

w.close()

#normalize the x,y and z data
f = open("consolidated.csv")
for line in f:
	data = line.split(",")
	x = int(data[0])
	y = int(data[1])
	z = int(data[2])
	values.add(x)
	values.add(y)
	values.add(z)

	if x < 10 or y < 10 or z < 10 or x > 2000 or y > 2000 or z > 2000:
		print line

maxValue = max(values)
minValue = min(values)

slope = 2.0 / (maxValue - minValue)
intercept = 1.0 - (slope * maxValue)

print slope
print intercept
print maxValue
print minValue

w = open("normalized.csv", "w")
f.seek(0)
for line in f:
	data = line.split(",")
	nData = [str(int(datum)*slope + intercept) for datum in data[:3]]
	w.write(",".join(nData))
	w.write(",")
	w.write(",".join(data[3:]))

f.close()
w.close()


