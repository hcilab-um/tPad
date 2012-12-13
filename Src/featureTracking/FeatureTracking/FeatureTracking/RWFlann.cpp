#include "StdAfx.h"
#include "RWFlann.h"


RWFlann::RWFlann(void)
{
}


RWFlann::~RWFlann(void)
{
}

#include <iostream>
#include <fstream>
#include <boost/archive/binary_oarchive.hpp>
#include <boost/archive/binary_iarchive.hpp>
#include <boost/serialization/split_free.hpp>
#include <boost/serialization/vector.hpp>

using namespace std;
using namespace cv;

BOOST_SERIALIZATION_SPLIT_FREE(Mat)
namespace boost {
namespace serialization {

/*** Mat ***/
template<class Archive>
void save(Archive & ar, const Mat& m, const unsigned int version)
{
size_t elemSize = m.elemSize(), elemType = m.type();

ar & m.cols;
ar & m.rows;
ar & elemSize;
ar & elemType; // element type.
size_t dataSize = m.cols * m.rows * m.elemSize();

//cout << "Writing matrix data rows, cols, elemSize, type, datasize: (" <<
m.rows << "," << m.cols << "," << m.elemSize() << "," << m.type() << "," <<
dataSize << ")" << endl;

for (size_t dc = 0; dc < dataSize; ++dc) {
ar & m.data[dc];
}
}

template<class Archive>
void load(Archive & ar, Mat& m, const unsigned int version)
{
int cols, rows;
size_t elemSize, elemType;

ar & cols;
ar & rows;
ar & elemSize;
ar & elemType;

m.create(rows, cols, elemType);
size_t dataSize = m.cols * m.rows * elemSize;

//cout << "reading matrix data rows, cols, elemSize, type, datasize: ("
<< m.rows << "," << m.cols << "," << m.elemSize() << "," << m.type() << "," <<
dataSize << ")" << endl;

for (size_t dc = 0; dc < dataSize; ++dc) {
ar & m.data[dc];
}
}

template<class Archive>
void serialize(Archive & ar, KeyPoint & k, const unsigned int version)
{
ar & k.pt;
ar & k.size;
ar & k.angle;
ar & k.response;
ar & k.octave;
ar & k.class_id;
}

template<class Archive>
void serialize(Archive & ar, Point2f & p, const unsigned int version)
{
ar & p.x;
ar & p.y;
}

}
}


class SerializableDescriptorMatcher : public DescriptorMatcher {

friend class boost::serialization::access;

template<class Archive>
void serialize(Archive & ar, const unsigned int version) {
ar & trainDescCollection;
}

public:
static Ptr<DescriptorMatcher> create( const string& descriptorMatcherType
);
};

class SerializableFlannBasedMatcher : public FlannBasedMatcher {

friend class boost::serialization::access;

template<class Archive>
void serialize(Archive & ar, const unsigned int version) {
//ar &
boost::serialization::base_object<SerializableDescriptorMatcher>(*this);
ar & trainDescCollection;
}

public:
SerializableFlannBasedMatcher()
: FlannBasedMatcher (new flann::KDTreeIndexParams(), new
flann::SearchParams())
{
//cout << "called SerializableFlannBasedMatcher()" << endl;
}
};

---------------------------------------

Ptr<DescriptorMatcher> SerializableDescriptorMatcher::create( const string&
descriptorMatcherType )
{
DescriptorMatcher* dm = 0;
if( !descriptorMatcherType.compare( "FlannBased" ) )
{
dm = new SerializableFlannBasedMatcher();
} else {
dm = DescriptorMatcher::create(descriptorMatcherType);
}

return dm;
}
---------------------------------------

So, matrices can be written and read as following:

---------------------------------------
void saveMat(Mat& m, string filename) {
ofstream ofs(filename.c_str());
boost::archive::binary_oarchive oa(ofs);
//boost::archive::text_oarchive oa(ofs);
oa << m;
}

void loadMat(Mat& m, string filename) {
std::ifstream ifs(filename.c_str());
boost::archive::binary_iarchive ia(ifs);
//boost::archive::text_iarchive ia(ifs);
ia >> m;
}
---------------------------------------

The flann-based descriptor matcher is a bit more complicated, as it needs to be
of the 'SerializableFlannBasedMatcher' variety:
SerializableFlannBasedMatcher* matcher =
SerializableDescriptorMatcher::create("FlannBased");

Data can be written and read as following:

---------------------------------------
void saveDescriptorMatcher(SerializableFlannBasedMatcher& dm, string
filename) {
ofstream ofs(filename.c_str());
boost::archive::binary_oarchive oa(ofs);
//boost::archive::text_oarchive oa(ofs);
oa << dm;
}

void loadDescriptorMatcher(SerializableFlannBasedMatcher& dm, string
filename) {
std::ifstream ifs(filename.c_str());
boost::archive::binary_iarchive ia(ifs);
//boost::archive::text_iarchive ia(ifs);
ia >> dm;
}
-------------------------------------