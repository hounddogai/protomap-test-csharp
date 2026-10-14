namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan segment is in its life cycle.</summary>
public enum RollingFloorPlanSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One rolling floor plan segment, with what the planners need to act on it.</summary>
public sealed class RollingFloorPlanSegment(string code, string name, string currency, string title, string ownerId, double ratio, double rating, int windowDays, string label, int endsAt, int revision, RollingFloorPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public double Ratio { get; set; } = ratio;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int Revision { get; set; } = revision;
    public RollingFloorPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan segment in status idle.</summary>
    public static RollingFloorPlanSegment NewRollingFloorPlanSegment(string code, string name)
    {
        return new RollingFloorPlanSegment(
            code,
            name,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            "",
            0,
            0,
            RollingFloorPlanSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanSegmentStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanSegmentStatus.Idle:
                return next == RollingFloorPlanSegmentStatus.Warming || next == RollingFloorPlanSegmentStatus.Stopped;
            case RollingFloorPlanSegmentStatus.Warming:
                switch (next)
                {
                    case RollingFloorPlanSegmentStatus.Ready:
                        return true;
                }
                return false;
            case RollingFloorPlanSegmentStatus.Ready:
                return next == RollingFloorPlanSegmentStatus.Stopped;
            case RollingFloorPlanSegmentStatus.Draining:
                return next == RollingFloorPlanSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"rolling floor plan segment"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Rating * (double)WindowDays, 2);
    }

    public RollingFloorPlanSegment Copy()
    {
        return new RollingFloorPlanSegment(
            Code,
            Name,
            Currency,
            Title,
            OwnerId,
            Ratio,
            Rating,
            WindowDays,
            Label,
            EndsAt,
            Revision,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
