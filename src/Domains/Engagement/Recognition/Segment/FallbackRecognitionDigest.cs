namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback recognition digest is in its life cycle.</summary>
public enum FallbackRecognitionDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback recognition digest. Its code is unique within a tenant.</summary>
public sealed class FallbackRecognitionDigest(string code, string label, double ceiling, int createdAt, int quantity, double rating, FallbackRecognitionDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public double Rating { get; set; } = rating;
    public FallbackRecognitionDigestStatus Status { get; set; } = status;

    /// <summary>Starts a fallback recognition digest in status idle.</summary>
    public static FallbackRecognitionDigest NewFallbackRecognitionDigest(string code, string label)
    {
        return new FallbackRecognitionDigest(code, label, 0.0, 0, 0, 0.0, FallbackRecognitionDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRecognitionDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRecognitionDigestStatus next)
    {
        switch (Status)
        {
            case FallbackRecognitionDigestStatus.Idle:
                switch (next)
                {
                    case FallbackRecognitionDigestStatus.Warming:
                    case FallbackRecognitionDigestStatus.Ready:
                        return true;
                }
                return false;
            case FallbackRecognitionDigestStatus.Warming:
                return next == FallbackRecognitionDigestStatus.Stopped;
            case FallbackRecognitionDigestStatus.Ready:
                return next == FallbackRecognitionDigestStatus.Stopped;
            case FallbackRecognitionDigestStatus.Draining:
                switch (next)
                {
                    case FallbackRecognitionDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackRecognitionDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback recognition digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"fallback recognition digest"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Quantity * (double)CreatedAt, 2);
    }

    public FallbackRecognitionDigest Copy()
    {
        return new FallbackRecognitionDigest(Code, Label, Ceiling, CreatedAt, Quantity, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
