namespace Acme.Workforce.Talent.Mobility.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy mobility segment is in its life cycle.</summary>
public enum LegacyMobilitySegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy mobility segment as the mobility team keeps it.</summary>
public sealed class LegacyMobilitySegment(string code, string title, int durationMinutes, double ceiling, double weight, string grade, string costCenter, string department, int limit, string ownerId, double ratio, LegacyMobilitySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public string Grade { get; set; } = grade;
    public string CostCenter { get; set; } = costCenter;
    public string Department { get; set; } = department;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public LegacyMobilitySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy mobility segment in status idle.</summary>
    public static LegacyMobilitySegment NewLegacyMobilitySegment(string code, string title)
    {
        return new LegacyMobilitySegment(
            code,
            title,
            0,
            0.0,
            0.0,
            "",
            "",
            "",
            0,
            "",
            0.0,
            LegacyMobilitySegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyMobilitySegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyMobilitySegmentStatus next)
    {
        switch (Status)
        {
            case LegacyMobilitySegmentStatus.Idle:
                switch (next)
                {
                    case LegacyMobilitySegmentStatus.Warming:
                        return true;
                }
                return false;
            case LegacyMobilitySegmentStatus.Warming:
                switch (next)
                {
                    case LegacyMobilitySegmentStatus.Ready:
                    case LegacyMobilitySegmentStatus.Draining:
                        return true;
                }
                return false;
            case LegacyMobilitySegmentStatus.Ready:
                return next == LegacyMobilitySegmentStatus.Draining || next == LegacyMobilitySegmentStatus.Stopped;
            case LegacyMobilitySegmentStatus.Draining:
                return next == LegacyMobilitySegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LegacyMobilitySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy mobility segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy mobility segment"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Weight * Ceiling, 2);
    }

    public LegacyMobilitySegment Copy()
    {
        return new LegacyMobilitySegment(
            Code,
            Title,
            DurationMinutes,
            Ceiling,
            Weight,
            Grade,
            CostCenter,
            Department,
            Limit,
            OwnerId,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
