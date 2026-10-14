namespace Acme.Workforce.Compensation.MeritCycle.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling merit cycle segment is in its life cycle.</summary>
public enum RollingMeritCycleSegmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a rolling merit cycle segment and its current status.</summary>
public sealed class RollingMeritCycleSegment(string code, string label, List<string> tags, double maximum, double floorValue, int batchSize, RollingMeritCycleSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public RollingMeritCycleSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling merit cycle segment in status draft.</summary>
    public static RollingMeritCycleSegment NewRollingMeritCycleSegment(string code, string label)
    {
        return new RollingMeritCycleSegment(
            code,
            label,
            new List<string>(),
            0.0,
            0.0,
            0,
            RollingMeritCycleSegmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMeritCycleSegmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMeritCycleSegmentStatus next)
    {
        switch (Status)
        {
            case RollingMeritCycleSegmentStatus.Draft:
                return next == RollingMeritCycleSegmentStatus.Active;
            case RollingMeritCycleSegmentStatus.Pending:
                switch (next)
                {
                    case RollingMeritCycleSegmentStatus.Suspended:
                    case RollingMeritCycleSegmentStatus.Closed:
                        return true;
                }
                return false;
            case RollingMeritCycleSegmentStatus.Active:
                switch (next)
                {
                    case RollingMeritCycleSegmentStatus.Suspended:
                    case RollingMeritCycleSegmentStatus.Closed:
                        return true;
                }
                return false;
            case RollingMeritCycleSegmentStatus.Suspended:
                return next == RollingMeritCycleSegmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingMeritCycleSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling merit cycle segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedExposure()
    {
        return Math.Round(FloorValue * Maximum, 2);
    }

    public RollingMeritCycleSegment Copy()
    {
        return new RollingMeritCycleSegment(Code, Label, Tags, Maximum, FloorValue, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
