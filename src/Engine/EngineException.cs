namespace Lpdf.Engine;

/// <summary>
/// Thrown when the lpdf engine returns a layout or parse error, a render runs past its timeout or its memory cap, or the
/// engine crashes.
/// </summary>
public sealed class EngineException : Exception
{
    /// <summary>The message of a render that crashes the engine, the same in every SDK of Lpdf.</summary>
    internal const string Crashed =
        "The engine crashed during the render. This is a bug in Lpdf, or the render ran out of memory before its cap " +
        "stopped it: please report it with the document at https://github.com/lpdfio/lpdf/issues.";

    /// <summary>An engine error with the message the engine gave.</summary>
    public EngineException(string message) : base(message) { }

    internal EngineException(string message, Exception innerException) : base(message, innerException) { }
}
