namespace Acme.Workforce.Engagement.Recognition.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual recognition segment is in its life cycle.</summary>
public enum ManualRecognitionSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual recognition segment as the recognition team keeps it.</summary>
public sealed class ManualRecognitionSegment(string code, string title, string name, string label, double amount, ManualRecognitionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public ManualRecognitionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a manual recognition segment in status draft.</summary>
    public static ManualRecognitionSegment NewManualRecognitionSegment(string code, string title)
    {
        return new ManualRecognitionSegment(code, title, "", "", 0.0, ManualRecognitionSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRecognitionSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRecognitionSegmentStatus next)
    {
        switch (Status)
        {
            case ManualRecognitionSegmentStatus.Draft:
                switch (next)
                {
                    case ManualRecognitionSegmentStatus.Pending:
                    case ManualRecognitionSegmentStatus.Active:
                        return true;
                }
                return false;
            case ManualRecognitionSegmentStatus.Pending:
                return next == ManualRecognitionSegmentStatus.Suspended || next == ManualRecognitionSegmentStatus.Closed;
            case ManualRecognitionSegmentStatus.Active:
                return next == ManualRecognitionSegmentStatus.Suspended || next == ManualRecognitionSegmentStatus.Closed;
            case ManualRecognitionSegmentStatus.Suspended:
                switch (next)
                {
                    case ManualRecognitionSegmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualRecognitionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual recognition segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"manual recognition segment"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ManualRecognitionSegment Copy()
    {
        return new ManualRecognitionSegment(Code, Title, Name, Label, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
