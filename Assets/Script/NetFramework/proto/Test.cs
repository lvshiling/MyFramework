// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: test.proto
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Msg {

  /// <summary>Holder for reflection information generated from test.proto</summary>
  public static partial class TestReflection {

    #region Descriptor
    /// <summary>File descriptor for test.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TestReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cgp0ZXN0LnByb3RvEgNtc2ciOAoSVGVzdFNpZ25VcFJlc3BvbnNlEhEKCWVy",
            "cm9yQ29kZRgBIAEoBRIPCgd2ZXJzaW9uGAIgASgCYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Msg.TestSignUpResponse), global::Msg.TestSignUpResponse.Parser, new[]{ "ErrorCode", "Version" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class TestSignUpResponse : pb::IMessage<TestSignUpResponse> {
    private static readonly pb::MessageParser<TestSignUpResponse> _parser = new pb::MessageParser<TestSignUpResponse>(() => new TestSignUpResponse());
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<TestSignUpResponse> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Msg.TestReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestSignUpResponse() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestSignUpResponse(TestSignUpResponse other) : this() {
      errorCode_ = other.errorCode_;
      version_ = other.version_;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public TestSignUpResponse Clone() {
      return new TestSignUpResponse(this);
    }

    /// <summary>Field number for the "errorCode" field.</summary>
    public const int ErrorCodeFieldNumber = 1;
    private int errorCode_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ErrorCode {
      get { return errorCode_; }
      set {
        errorCode_ = value;
      }
    }

    /// <summary>Field number for the "version" field.</summary>
    public const int VersionFieldNumber = 2;
    private float version_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public float Version {
      get { return version_; }
      set {
        version_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as TestSignUpResponse);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(TestSignUpResponse other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ErrorCode != other.ErrorCode) return false;
      if (Version != other.Version) return false;
      return true;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (ErrorCode != 0) hash ^= ErrorCode.GetHashCode();
      if (Version != 0F) hash ^= Version.GetHashCode();
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (ErrorCode != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(ErrorCode);
      }
      if (Version != 0F) {
        output.WriteRawTag(21);
        output.WriteFloat(Version);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (ErrorCode != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ErrorCode);
      }
      if (Version != 0F) {
        size += 1 + 4;
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(TestSignUpResponse other) {
      if (other == null) {
        return;
      }
      if (other.ErrorCode != 0) {
        ErrorCode = other.ErrorCode;
      }
      if (other.Version != 0F) {
        Version = other.Version;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            input.SkipLastField();
            break;
          case 8: {
            ErrorCode = input.ReadInt32();
            break;
          }
          case 21: {
            Version = input.ReadFloat();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
