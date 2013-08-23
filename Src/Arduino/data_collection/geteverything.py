w = open("alldata.csv", "w")
f = open("running/normalized.csv")

for line in f:
	w.write(line)

f.close()
f = open("sitting/normalized.csv")
for line in f:
	w.write(line)

f.close()
f = open("walking/normalized.csv")
for line in f:
	w.write(line)

f.close()
w.close()