namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition segment is in its life cycle.</summary>
public enum RecognitionSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A recognition segment as the recognition team keeps it.</summary>
public sealed class RecognitionSegment(string code, string title, bool pinned, string grade, double minimum, string segment, string contactEmail, double discount, RecognitionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Pinned { get; set; } = pinned;
    public string Grade { get; set; } = grade;
    public double Minimum { get; set; } = minimum;
    public string Segment { get; set; } = segment;
    public string ContactEmail { get; set; } = contactEmail;
    public double Discount { get; set; } = discount;
    public RecognitionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a recognition segment in status idle.</summary>
    public static RecognitionSegment NewRecognitionSegment(string code, string title)
    {
        return new RecognitionSegment(code, title, false, "", 0.0, "", "", 0.0, RecognitionSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionSegmentStatus next)
    {
        switch (Status)
        {
            case RecognitionSegmentStatus.Idle:
                return next == RecognitionSegmentStatus.Stopped;
            case RecognitionSegmentStatus.Warming:
                switch (next)
                {
                    case RecognitionSegmentStatus.Ready:
                    case RecognitionSegmentStatus.Draining:
                        return true;
                }
                return false;
            case RecognitionSegmentStatus.Ready:
                return next == RecognitionSegmentStatus.Stopped;
            case RecognitionSegmentStatus.Draining:
                switch (next)
                {
                    case RecognitionSegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"recognition segment"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount + Minimum, 2);
    }

    public RecognitionSegment Copy()
    {
        return new RecognitionSegment(Code, Title, Pinned, Grade, Minimum, Segment, ContactEmail, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
