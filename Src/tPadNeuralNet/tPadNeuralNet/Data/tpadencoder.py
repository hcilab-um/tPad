def transform(slope, intercept, value):
    return slope*value + intercept

x = []
y = []
z = []



f = open("tpad cleaned.csv")
for line in f.readlines():
    # print line
    temp = line.split(",")
    x.append(int(temp[0]))
    y.append(int(temp[1]))
    z.append(int(temp[2]))


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
for line in f.readlines():
    split = line.strip().split(",")
    split[0] = str(transform(slope, intercept, int(split[0])))
    split[1] = str(transform(slope, intercept, int(split[1])))
    split[2] = str(transform(slope, intercept, int(split[2])))
    if "down" == split[3]:
        split[3] = "0.0"
    else:
        split[3] = "1.0"
    w.write(",".join(split) + "\n")

print "End of processing"



