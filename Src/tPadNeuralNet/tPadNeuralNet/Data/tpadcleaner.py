cleaned = open("tpad cleaned.csv", mode="w")
consolidated = open("tpad consolidated.csv")

consolidated.readline()
for line in consolidated:
    split = line.split(",")
    del split[4:10]
    del split[0]
    
    cleaned.write(",".join(split));

print "End of processing"