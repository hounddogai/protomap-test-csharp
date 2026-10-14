namespace Acme.Workforce.Payrollops.Deduction.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict deduction segment is in its life cycle.</summary>
public enum StrictDeductionSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A strict deduction segment as the deduction team keeps it.</summary>
public sealed class StrictDeductionSegment(string code, string name, string title, double floorValue, int revision, int durationMinutes, double rating, double hours, int expiresAt, bool approved, StrictDeductionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rating { get; set; } = rating;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Approved { get; set; } = approved;
    public StrictDeductionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict deduction segment in status new.</summary>
    public static StrictDeductionSegment NewStrictDeductionSegment(string code, string name)
    {
        return new StrictDeductionSegment(
            code,
            name,
            "",
            0.0,
            0,
            0,
            0.0,
            0.0,
            0,
            false,
            StrictDeductionSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictDeductionSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictDeductionSegmentStatus next)
    {
        switch (Status)
        {
            case StrictDeductionSegmentStatus.New:
                return next == StrictDeductionSegmentStatus.Done;
            case StrictDeductionSegmentStatus.Queued:
                return next == StrictDeductionSegmentStatus.Running;
            case StrictDeductionSegmentStatus.Running:
                switch (next)
                {
                    case StrictDeductionSegmentStatus.Done:
                    case StrictDeductionSegmentStatus.Failed:
                        return true;
                }
                return false;
            case StrictDeductionSegmentStatus.Done:
                switch (next)
                {
                    case StrictDeductionSegmentStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictDeductionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict deduction segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"strict deduction segment"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)DurationMinutes + (double)Revision, 2);
    }

    public StrictDeductionSegment Copy()
    {
        return new StrictDeductionSegment(
            Code,
            Name,
            Title,
            FloorValue,
            Revision,
            DurationMinutes,
            Rating,
            Hours,
            ExpiresAt,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
