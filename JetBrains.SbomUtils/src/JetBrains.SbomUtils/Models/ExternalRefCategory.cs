using System.Runtime.Serialization;

namespace JetBrains.SbomUtils.Models
{
  public enum ExternalRefCategory
  {
    OTHER,
    [EnumMember(Value = "PACKAGE-MANAGER")]
    PACKAGE_MANAGER,
    SECURITY,
  }
}