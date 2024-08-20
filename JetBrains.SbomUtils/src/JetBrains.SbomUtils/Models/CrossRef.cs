namespace JetBrains.SbomUtils.Models
{
  /// <summary>
  /// Cross reference details for the a URL reference
  /// </summary>
  public class CrossRef
  {
    /// <summary>
    /// True if the License SeeAlso URL points to a Wayback archive
    /// </summary>
    public bool IsWayBackLink { get; set; }

    /// <summary>
    /// True if the URL is a valid well formed URL
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Indicate a URL is still a live accessible location on the public internet
    /// </summary>
    public bool IsLive { get; set; }

    /// <summary>
    /// Status of a License List SeeAlso URL reference if it refers to a website that matches the license text.
    /// </summary>
    public string? Match { get; set; }

    /// <summary>
    /// The ordinal order of this element within a list
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public string? Timestamp { get; set; }

    /// <summary>
    /// URL Reference
    /// </summary>
    public string Url { get; set; } = String.Empty;
  }
}