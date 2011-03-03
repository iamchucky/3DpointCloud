// Generated by the protocol buffer compiler.  DO NOT EDIT!

#define INTERNAL_SUPPRESS_PROTOBUF_FIELD_DEPRECATION
#include "OOIDetectorInput.pb.h"
#include <google/protobuf/stubs/once.h>
#include <google/protobuf/io/coded_stream.h>
#include <google/protobuf/wire_format_lite_inl.h>
#include <google/protobuf/descriptor.h>
#include <google/protobuf/reflection_ops.h>
#include <google/protobuf/wire_format.h>

namespace Magic {
namespace OOIDetector {

namespace {

const ::google::protobuf::Descriptor* OOIDetectorInput_descriptor_ = NULL;
const ::google::protobuf::internal::GeneratedMessageReflection*
  OOIDetectorInput_reflection_ = NULL;
const ::google::protobuf::Descriptor* OOIDetectorInput_ThreeDCoord_descriptor_ = NULL;
const ::google::protobuf::internal::GeneratedMessageReflection*
  OOIDetectorInput_ThreeDCoord_reflection_ = NULL;

}  // namespace


void protobuf_AssignDesc_OOIDetectorInput_2eproto() {
  protobuf_AddDesc_OOIDetectorInput_2eproto();
  const ::google::protobuf::FileDescriptor* file =
    ::google::protobuf::DescriptorPool::generated_pool()->FindFileByName(
      "OOIDetectorInput.proto");
  GOOGLE_CHECK(file != NULL);
  OOIDetectorInput_descriptor_ = file->message_type(0);
  static const int OOIDetectorInput_offsets_[3] = {
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput, timestamp_),
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput, globalcoord_),
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput, robotcoord_),
  };
  OOIDetectorInput_reflection_ =
    new ::google::protobuf::internal::GeneratedMessageReflection(
      OOIDetectorInput_descriptor_,
      OOIDetectorInput::default_instance_,
      OOIDetectorInput_offsets_,
      GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput, _has_bits_[0]),
      GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput, _unknown_fields_),
      -1,
      ::google::protobuf::DescriptorPool::generated_pool(),
      ::google::protobuf::MessageFactory::generated_factory(),
      sizeof(OOIDetectorInput));
  OOIDetectorInput_ThreeDCoord_descriptor_ = OOIDetectorInput_descriptor_->nested_type(0);
  static const int OOIDetectorInput_ThreeDCoord_offsets_[3] = {
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput_ThreeDCoord, x_),
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput_ThreeDCoord, y_),
    GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput_ThreeDCoord, z_),
  };
  OOIDetectorInput_ThreeDCoord_reflection_ =
    new ::google::protobuf::internal::GeneratedMessageReflection(
      OOIDetectorInput_ThreeDCoord_descriptor_,
      OOIDetectorInput_ThreeDCoord::default_instance_,
      OOIDetectorInput_ThreeDCoord_offsets_,
      GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput_ThreeDCoord, _has_bits_[0]),
      GOOGLE_PROTOBUF_GENERATED_MESSAGE_FIELD_OFFSET(OOIDetectorInput_ThreeDCoord, _unknown_fields_),
      -1,
      ::google::protobuf::DescriptorPool::generated_pool(),
      ::google::protobuf::MessageFactory::generated_factory(),
      sizeof(OOIDetectorInput_ThreeDCoord));
}

namespace {

GOOGLE_PROTOBUF_DECLARE_ONCE(protobuf_AssignDescriptors_once_);
inline void protobuf_AssignDescriptorsOnce() {
  ::google::protobuf::GoogleOnceInit(&protobuf_AssignDescriptors_once_,
                 &protobuf_AssignDesc_OOIDetectorInput_2eproto);
}

void protobuf_RegisterTypes(const ::std::string&) {
  protobuf_AssignDescriptorsOnce();
  ::google::protobuf::MessageFactory::InternalRegisterGeneratedMessage(
    OOIDetectorInput_descriptor_, &OOIDetectorInput::default_instance());
  ::google::protobuf::MessageFactory::InternalRegisterGeneratedMessage(
    OOIDetectorInput_ThreeDCoord_descriptor_, &OOIDetectorInput_ThreeDCoord::default_instance());
}

}  // namespace

void protobuf_ShutdownFile_OOIDetectorInput_2eproto() {
  delete OOIDetectorInput::default_instance_;
  delete OOIDetectorInput_reflection_;
  delete OOIDetectorInput_ThreeDCoord::default_instance_;
  delete OOIDetectorInput_ThreeDCoord_reflection_;
}

void protobuf_AddDesc_OOIDetectorInput_2eproto() {
  static bool already_here = false;
  if (already_here) return;
  already_here = true;
  GOOGLE_PROTOBUF_VERIFY_VERSION;

  ::google::protobuf::DescriptorPool::InternalAddGeneratedFile(
    "\n\026OOIDetectorInput.proto\022\021Magic.OOIDetec"
    "tor\"\340\001\n\020OOIDetectorInput\022\021\n\ttimestamp\030\001 "
    "\002(\001\022D\n\013globalcoord\030\002 \002(\0132/.Magic.OOIDete"
    "ctor.OOIDetectorInput.ThreeDCoord\022C\n\nrob"
    "otcoord\030\003 \002(\0132/.Magic.OOIDetector.OOIDet"
    "ectorInput.ThreeDCoord\032.\n\013ThreeDCoord\022\t\n"
    "\001x\030\001 \002(\001\022\t\n\001y\030\002 \002(\001\022\t\n\001z\030\003 \002(\001", 270);
  ::google::protobuf::MessageFactory::InternalRegisterGeneratedFile(
    "OOIDetectorInput.proto", &protobuf_RegisterTypes);
  OOIDetectorInput::default_instance_ = new OOIDetectorInput();
  OOIDetectorInput_ThreeDCoord::default_instance_ = new OOIDetectorInput_ThreeDCoord();
  OOIDetectorInput::default_instance_->InitAsDefaultInstance();
  OOIDetectorInput_ThreeDCoord::default_instance_->InitAsDefaultInstance();
  ::google::protobuf::internal::OnShutdown(&protobuf_ShutdownFile_OOIDetectorInput_2eproto);
}

// Force AddDescriptors() to be called at static initialization time.
struct StaticDescriptorInitializer_OOIDetectorInput_2eproto {
  StaticDescriptorInitializer_OOIDetectorInput_2eproto() {
    protobuf_AddDesc_OOIDetectorInput_2eproto();
  }
} static_descriptor_initializer_OOIDetectorInput_2eproto_;


// ===================================================================

#ifndef _MSC_VER
const int OOIDetectorInput_ThreeDCoord::kXFieldNumber;
const int OOIDetectorInput_ThreeDCoord::kYFieldNumber;
const int OOIDetectorInput_ThreeDCoord::kZFieldNumber;
#endif  // !_MSC_VER

OOIDetectorInput_ThreeDCoord::OOIDetectorInput_ThreeDCoord() {
  SharedCtor();
}

void OOIDetectorInput_ThreeDCoord::InitAsDefaultInstance() {
}

OOIDetectorInput_ThreeDCoord::OOIDetectorInput_ThreeDCoord(const OOIDetectorInput_ThreeDCoord& from) {
  SharedCtor();
  MergeFrom(from);
}

void OOIDetectorInput_ThreeDCoord::SharedCtor() {
  _cached_size_ = 0;
  x_ = 0;
  y_ = 0;
  z_ = 0;
  ::memset(_has_bits_, 0, sizeof(_has_bits_));
}

OOIDetectorInput_ThreeDCoord::~OOIDetectorInput_ThreeDCoord() {
  SharedDtor();
}

void OOIDetectorInput_ThreeDCoord::SharedDtor() {
  if (this != default_instance_) {
  }
}

const ::google::protobuf::Descriptor* OOIDetectorInput_ThreeDCoord::descriptor() {
  protobuf_AssignDescriptorsOnce();
  return OOIDetectorInput_ThreeDCoord_descriptor_;
}

const OOIDetectorInput_ThreeDCoord& OOIDetectorInput_ThreeDCoord::default_instance() {
  if (default_instance_ == NULL) protobuf_AddDesc_OOIDetectorInput_2eproto();  return *default_instance_;
}

OOIDetectorInput_ThreeDCoord* OOIDetectorInput_ThreeDCoord::default_instance_ = NULL;

OOIDetectorInput_ThreeDCoord* OOIDetectorInput_ThreeDCoord::New() const {
  return new OOIDetectorInput_ThreeDCoord;
}

void OOIDetectorInput_ThreeDCoord::Clear() {
  if (_has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    x_ = 0;
    y_ = 0;
    z_ = 0;
  }
  ::memset(_has_bits_, 0, sizeof(_has_bits_));
  mutable_unknown_fields()->Clear();
}

bool OOIDetectorInput_ThreeDCoord::MergePartialFromCodedStream(
    ::google::protobuf::io::CodedInputStream* input) {
#define DO_(EXPRESSION) if (!(EXPRESSION)) return false
  ::google::protobuf::uint32 tag;
  while ((tag = input->ReadTag()) != 0) {
    switch (::google::protobuf::internal::WireFormatLite::GetTagFieldNumber(tag)) {
      // required double x = 1;
      case 1: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_FIXED64) {
          goto handle_uninterpreted;
        }
        DO_(::google::protobuf::internal::WireFormatLite::ReadDouble(
              input, &x_));
        _set_bit(0);
        if (input->ExpectTag(17)) goto parse_y;
        break;
      }
      
      // required double y = 2;
      case 2: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_FIXED64) {
          goto handle_uninterpreted;
        }
       parse_y:
        DO_(::google::protobuf::internal::WireFormatLite::ReadDouble(
              input, &y_));
        _set_bit(1);
        if (input->ExpectTag(25)) goto parse_z;
        break;
      }
      
      // required double z = 3;
      case 3: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_FIXED64) {
          goto handle_uninterpreted;
        }
       parse_z:
        DO_(::google::protobuf::internal::WireFormatLite::ReadDouble(
              input, &z_));
        _set_bit(2);
        if (input->ExpectAtEnd()) return true;
        break;
      }
      
      default: {
      handle_uninterpreted:
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) ==
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_END_GROUP) {
          return true;
        }
        DO_(::google::protobuf::internal::WireFormat::SkipField(
              input, tag, mutable_unknown_fields()));
        break;
      }
    }
  }
  return true;
#undef DO_
}

void OOIDetectorInput_ThreeDCoord::SerializeWithCachedSizes(
    ::google::protobuf::io::CodedOutputStream* output) const {
  ::google::protobuf::uint8* raw_buffer = output->GetDirectBufferForNBytesAndAdvance(_cached_size_);
  if (raw_buffer != NULL) {
    OOIDetectorInput_ThreeDCoord::SerializeWithCachedSizesToArray(raw_buffer);
    return;
  }
  
  // required double x = 1;
  if (_has_bit(0)) {
    ::google::protobuf::internal::WireFormatLite::WriteDouble(1, this->x(), output);
  }
  
  // required double y = 2;
  if (_has_bit(1)) {
    ::google::protobuf::internal::WireFormatLite::WriteDouble(2, this->y(), output);
  }
  
  // required double z = 3;
  if (_has_bit(2)) {
    ::google::protobuf::internal::WireFormatLite::WriteDouble(3, this->z(), output);
  }
  
  if (!unknown_fields().empty()) {
    ::google::protobuf::internal::WireFormat::SerializeUnknownFields(
        unknown_fields(), output);
  }
}

::google::protobuf::uint8* OOIDetectorInput_ThreeDCoord::SerializeWithCachedSizesToArray(
    ::google::protobuf::uint8* target) const {
  // required double x = 1;
  if (_has_bit(0)) {
    target = ::google::protobuf::internal::WireFormatLite::WriteDoubleToArray(1, this->x(), target);
  }
  
  // required double y = 2;
  if (_has_bit(1)) {
    target = ::google::protobuf::internal::WireFormatLite::WriteDoubleToArray(2, this->y(), target);
  }
  
  // required double z = 3;
  if (_has_bit(2)) {
    target = ::google::protobuf::internal::WireFormatLite::WriteDoubleToArray(3, this->z(), target);
  }
  
  if (!unknown_fields().empty()) {
    target = ::google::protobuf::internal::WireFormat::SerializeUnknownFieldsToArray(
        unknown_fields(), target);
  }
  return target;
}

int OOIDetectorInput_ThreeDCoord::ByteSize() const {
  int total_size = 0;
  
  if (_has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    // required double x = 1;
    if (has_x()) {
      total_size += 1 + 8;
    }
    
    // required double y = 2;
    if (has_y()) {
      total_size += 1 + 8;
    }
    
    // required double z = 3;
    if (has_z()) {
      total_size += 1 + 8;
    }
    
  }
  if (!unknown_fields().empty()) {
    total_size +=
      ::google::protobuf::internal::WireFormat::ComputeUnknownFieldsSize(
        unknown_fields());
  }
  _cached_size_ = total_size;
  return total_size;
}

void OOIDetectorInput_ThreeDCoord::MergeFrom(const ::google::protobuf::Message& from) {
  GOOGLE_CHECK_NE(&from, this);
  const OOIDetectorInput_ThreeDCoord* source =
    ::google::protobuf::internal::dynamic_cast_if_available<const OOIDetectorInput_ThreeDCoord*>(
      &from);
  if (source == NULL) {
    ::google::protobuf::internal::ReflectionOps::Merge(from, this);
  } else {
    MergeFrom(*source);
  }
}

void OOIDetectorInput_ThreeDCoord::MergeFrom(const OOIDetectorInput_ThreeDCoord& from) {
  GOOGLE_CHECK_NE(&from, this);
  if (from._has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    if (from._has_bit(0)) {
      set_x(from.x());
    }
    if (from._has_bit(1)) {
      set_y(from.y());
    }
    if (from._has_bit(2)) {
      set_z(from.z());
    }
  }
  mutable_unknown_fields()->MergeFrom(from.unknown_fields());
}

void OOIDetectorInput_ThreeDCoord::CopyFrom(const ::google::protobuf::Message& from) {
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

void OOIDetectorInput_ThreeDCoord::CopyFrom(const OOIDetectorInput_ThreeDCoord& from) {
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

bool OOIDetectorInput_ThreeDCoord::IsInitialized() const {
  if ((_has_bits_[0] & 0x00000007) != 0x00000007) return false;
  
  return true;
}

void OOIDetectorInput_ThreeDCoord::Swap(OOIDetectorInput_ThreeDCoord* other) {
  if (other != this) {
    std::swap(x_, other->x_);
    std::swap(y_, other->y_);
    std::swap(z_, other->z_);
    std::swap(_has_bits_[0], other->_has_bits_[0]);
    _unknown_fields_.Swap(&other->_unknown_fields_);
    std::swap(_cached_size_, other->_cached_size_);
  }
}

::google::protobuf::Metadata OOIDetectorInput_ThreeDCoord::GetMetadata() const {
  protobuf_AssignDescriptorsOnce();
  ::google::protobuf::Metadata metadata;
  metadata.descriptor = OOIDetectorInput_ThreeDCoord_descriptor_;
  metadata.reflection = OOIDetectorInput_ThreeDCoord_reflection_;
  return metadata;
}


// -------------------------------------------------------------------

#ifndef _MSC_VER
const int OOIDetectorInput::kTimestampFieldNumber;
const int OOIDetectorInput::kGlobalcoordFieldNumber;
const int OOIDetectorInput::kRobotcoordFieldNumber;
#endif  // !_MSC_VER

OOIDetectorInput::OOIDetectorInput() {
  SharedCtor();
}

void OOIDetectorInput::InitAsDefaultInstance() {
  globalcoord_ = const_cast< ::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord*>(&::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::default_instance());
  robotcoord_ = const_cast< ::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord*>(&::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::default_instance());
}

OOIDetectorInput::OOIDetectorInput(const OOIDetectorInput& from) {
  SharedCtor();
  MergeFrom(from);
}

void OOIDetectorInput::SharedCtor() {
  _cached_size_ = 0;
  timestamp_ = 0;
  globalcoord_ = NULL;
  robotcoord_ = NULL;
  ::memset(_has_bits_, 0, sizeof(_has_bits_));
}

OOIDetectorInput::~OOIDetectorInput() {
  SharedDtor();
}

void OOIDetectorInput::SharedDtor() {
  if (this != default_instance_) {
    delete globalcoord_;
    delete robotcoord_;
  }
}

const ::google::protobuf::Descriptor* OOIDetectorInput::descriptor() {
  protobuf_AssignDescriptorsOnce();
  return OOIDetectorInput_descriptor_;
}

const OOIDetectorInput& OOIDetectorInput::default_instance() {
  if (default_instance_ == NULL) protobuf_AddDesc_OOIDetectorInput_2eproto();  return *default_instance_;
}

OOIDetectorInput* OOIDetectorInput::default_instance_ = NULL;

OOIDetectorInput* OOIDetectorInput::New() const {
  return new OOIDetectorInput;
}

void OOIDetectorInput::Clear() {
  if (_has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    timestamp_ = 0;
    if (_has_bit(1)) {
      if (globalcoord_ != NULL) globalcoord_->::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::Clear();
    }
    if (_has_bit(2)) {
      if (robotcoord_ != NULL) robotcoord_->::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::Clear();
    }
  }
  ::memset(_has_bits_, 0, sizeof(_has_bits_));
  mutable_unknown_fields()->Clear();
}

bool OOIDetectorInput::MergePartialFromCodedStream(
    ::google::protobuf::io::CodedInputStream* input) {
#define DO_(EXPRESSION) if (!(EXPRESSION)) return false
  ::google::protobuf::uint32 tag;
  while ((tag = input->ReadTag()) != 0) {
    switch (::google::protobuf::internal::WireFormatLite::GetTagFieldNumber(tag)) {
      // required double timestamp = 1;
      case 1: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_FIXED64) {
          goto handle_uninterpreted;
        }
        DO_(::google::protobuf::internal::WireFormatLite::ReadDouble(
              input, &timestamp_));
        _set_bit(0);
        if (input->ExpectTag(18)) goto parse_globalcoord;
        break;
      }
      
      // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord globalcoord = 2;
      case 2: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_LENGTH_DELIMITED) {
          goto handle_uninterpreted;
        }
       parse_globalcoord:
        DO_(::google::protobuf::internal::WireFormatLite::ReadMessageNoVirtual(
             input, mutable_globalcoord()));
        if (input->ExpectTag(26)) goto parse_robotcoord;
        break;
      }
      
      // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord robotcoord = 3;
      case 3: {
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) !=
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_LENGTH_DELIMITED) {
          goto handle_uninterpreted;
        }
       parse_robotcoord:
        DO_(::google::protobuf::internal::WireFormatLite::ReadMessageNoVirtual(
             input, mutable_robotcoord()));
        if (input->ExpectAtEnd()) return true;
        break;
      }
      
      default: {
      handle_uninterpreted:
        if (::google::protobuf::internal::WireFormatLite::GetTagWireType(tag) ==
            ::google::protobuf::internal::WireFormatLite::WIRETYPE_END_GROUP) {
          return true;
        }
        DO_(::google::protobuf::internal::WireFormat::SkipField(
              input, tag, mutable_unknown_fields()));
        break;
      }
    }
  }
  return true;
#undef DO_
}

void OOIDetectorInput::SerializeWithCachedSizes(
    ::google::protobuf::io::CodedOutputStream* output) const {
  ::google::protobuf::uint8* raw_buffer = output->GetDirectBufferForNBytesAndAdvance(_cached_size_);
  if (raw_buffer != NULL) {
    OOIDetectorInput::SerializeWithCachedSizesToArray(raw_buffer);
    return;
  }
  
  // required double timestamp = 1;
  if (_has_bit(0)) {
    ::google::protobuf::internal::WireFormatLite::WriteDouble(1, this->timestamp(), output);
  }
  
  // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord globalcoord = 2;
  if (_has_bit(1)) {
    ::google::protobuf::internal::WireFormatLite::WriteMessageNoVirtual(
      2, this->globalcoord(), output);
  }
  
  // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord robotcoord = 3;
  if (_has_bit(2)) {
    ::google::protobuf::internal::WireFormatLite::WriteMessageNoVirtual(
      3, this->robotcoord(), output);
  }
  
  if (!unknown_fields().empty()) {
    ::google::protobuf::internal::WireFormat::SerializeUnknownFields(
        unknown_fields(), output);
  }
}

::google::protobuf::uint8* OOIDetectorInput::SerializeWithCachedSizesToArray(
    ::google::protobuf::uint8* target) const {
  // required double timestamp = 1;
  if (_has_bit(0)) {
    target = ::google::protobuf::internal::WireFormatLite::WriteDoubleToArray(1, this->timestamp(), target);
  }
  
  // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord globalcoord = 2;
  if (_has_bit(1)) {
    target = ::google::protobuf::internal::WireFormatLite::
      WriteMessageNoVirtualToArray(
        2, this->globalcoord(), target);
  }
  
  // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord robotcoord = 3;
  if (_has_bit(2)) {
    target = ::google::protobuf::internal::WireFormatLite::
      WriteMessageNoVirtualToArray(
        3, this->robotcoord(), target);
  }
  
  if (!unknown_fields().empty()) {
    target = ::google::protobuf::internal::WireFormat::SerializeUnknownFieldsToArray(
        unknown_fields(), target);
  }
  return target;
}

int OOIDetectorInput::ByteSize() const {
  int total_size = 0;
  
  if (_has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    // required double timestamp = 1;
    if (has_timestamp()) {
      total_size += 1 + 8;
    }
    
    // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord globalcoord = 2;
    if (has_globalcoord()) {
      total_size += 1 +
        ::google::protobuf::internal::WireFormatLite::MessageSizeNoVirtual(
          this->globalcoord());
    }
    
    // required .Magic.OOIDetector.OOIDetectorInput.ThreeDCoord robotcoord = 3;
    if (has_robotcoord()) {
      total_size += 1 +
        ::google::protobuf::internal::WireFormatLite::MessageSizeNoVirtual(
          this->robotcoord());
    }
    
  }
  if (!unknown_fields().empty()) {
    total_size +=
      ::google::protobuf::internal::WireFormat::ComputeUnknownFieldsSize(
        unknown_fields());
  }
  _cached_size_ = total_size;
  return total_size;
}

void OOIDetectorInput::MergeFrom(const ::google::protobuf::Message& from) {
  GOOGLE_CHECK_NE(&from, this);
  const OOIDetectorInput* source =
    ::google::protobuf::internal::dynamic_cast_if_available<const OOIDetectorInput*>(
      &from);
  if (source == NULL) {
    ::google::protobuf::internal::ReflectionOps::Merge(from, this);
  } else {
    MergeFrom(*source);
  }
}

void OOIDetectorInput::MergeFrom(const OOIDetectorInput& from) {
  GOOGLE_CHECK_NE(&from, this);
  if (from._has_bits_[0 / 32] & (0xffu << (0 % 32))) {
    if (from._has_bit(0)) {
      set_timestamp(from.timestamp());
    }
    if (from._has_bit(1)) {
      mutable_globalcoord()->::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::MergeFrom(from.globalcoord());
    }
    if (from._has_bit(2)) {
      mutable_robotcoord()->::Magic::OOIDetector::OOIDetectorInput_ThreeDCoord::MergeFrom(from.robotcoord());
    }
  }
  mutable_unknown_fields()->MergeFrom(from.unknown_fields());
}

void OOIDetectorInput::CopyFrom(const ::google::protobuf::Message& from) {
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

void OOIDetectorInput::CopyFrom(const OOIDetectorInput& from) {
  if (&from == this) return;
  Clear();
  MergeFrom(from);
}

bool OOIDetectorInput::IsInitialized() const {
  if ((_has_bits_[0] & 0x00000007) != 0x00000007) return false;
  
  if (has_globalcoord()) {
    if (!this->globalcoord().IsInitialized()) return false;
  }
  if (has_robotcoord()) {
    if (!this->robotcoord().IsInitialized()) return false;
  }
  return true;
}

void OOIDetectorInput::Swap(OOIDetectorInput* other) {
  if (other != this) {
    std::swap(timestamp_, other->timestamp_);
    std::swap(globalcoord_, other->globalcoord_);
    std::swap(robotcoord_, other->robotcoord_);
    std::swap(_has_bits_[0], other->_has_bits_[0]);
    _unknown_fields_.Swap(&other->_unknown_fields_);
    std::swap(_cached_size_, other->_cached_size_);
  }
}

::google::protobuf::Metadata OOIDetectorInput::GetMetadata() const {
  protobuf_AssignDescriptorsOnce();
  ::google::protobuf::Metadata metadata;
  metadata.descriptor = OOIDetectorInput_descriptor_;
  metadata.reflection = OOIDetectorInput_reflection_;
  return metadata;
}


}  // namespace OOIDetector
}  // namespace Magic
