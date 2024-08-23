namespace JetBrains.SbomUtils;

public class ValidationOptions
{
  public int Threads { get; set; } = Environment.ProcessorCount;
}