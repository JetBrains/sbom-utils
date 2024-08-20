using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace JetBrains.SbomUtils
{
  public static class Utils
  {
    [DebuggerStepThrough]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static R WithDispose<T, R>(this T disposable, Func<T, R> F) where T : IDisposable
    {
      using (disposable)
      {
        return F(disposable);
      }
    }
  
    [DebuggerStepThrough]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static async Task<R> WithDisposeAsync<T, R>(this T disposable, Func<T, Task<R>> F) where T : IDisposable
    {
      using (disposable)
      {
        return await F(disposable);
      }
    }
  
    /// <summary>
    /// <para>Rewinds the stream to the beginning so that it could be reused for reading.</para>
    /// <para>For example, this should be done to a <see cref="MemoryStream"/> after writing and before each reading.</para>
    /// <para>Fluent.</para>
    /// </summary>
    public static Stream Rewind([NotNull] this Stream thіs)
    {
      if (thіs == null)
        throw new ArgumentNullException(nameof(thіs));

      if (thіs.Position > 0)
        thіs.Seek(0, SeekOrigin.Begin);

      return thіs;
    }
  
    public static bool ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2)
    {
      return a1.SequenceEqual(a2);
    }
  }
}