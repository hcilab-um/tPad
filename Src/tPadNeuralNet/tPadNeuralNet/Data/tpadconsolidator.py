consolidated = open("tpad consolidated.csv", "w")


f = open("tpad down.csv")
consolidated.write(f.readline().strip() + ",direction\n")
for line in f.readlines():
	consolidated.write(line.strip() + ",down\n")

f = open("tpad up.csv")
f.readline()
for line in f.readlines():
	consolidated.write(line.strip() + ",up\n")

print "End of processing"