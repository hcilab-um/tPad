def transform(slope, intercept, value):
	return slope*value + intercept

x = []
y = []
z = []



f = open("tpad consolidated.csv")
f.readline()
for line in f.readlines():
	# print line
	temp = line.split(",")
	x.append(int(temp[1]))
	y.append(int(temp[2]))
	z.append(int(temp[3]))


minimum = min([min(x), min(y), min(z)])
maximum = max([max(x), max(y), max(z)])

slope = 2.0 / (maximum - minimum)
intercept = 1.0 - (slope*maximum)

print maximum
print minimum

print slope
print intercept

f.seek(0)
w = open("tpad encoded.csv", "w")
w.write(f.readline())
for line in f.readlines():
	parts = line.split(",")
	parts[1] = str(transform(slope, intercept, int(parts[1])))
	parts[2] = str(transform(slope, intercept, int(parts[2])))
	parts[3] = str(transform(slope, intercept, int(parts[3])))
	w.write(",".join(parts))



