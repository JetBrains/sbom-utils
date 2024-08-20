namespace JetBrains.SbomUtils
{
  public class SbomValidationException: Exception
  {
    public SbomValidationException(string message) : base(message) {}

    public SbomValidationException(string message, Exception innerException) : base(message, innerException) { }
  }
}