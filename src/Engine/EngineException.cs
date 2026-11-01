namespace Lpdf.Engine;

/// <summary>Thrown when the lpdf engine returns a layout or parse error, or a render runs past its timeout.</summary>
public sealed class EngineException : Exception
{
    /// <summary>An engine error with the message the engine gave.</summary>
    public EngineException(string message) : base(message) { }

    internal EngineException(string message, Exception innerException) : base(message, innerException) { }
}
