namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan segment is in its life cycle.</summary>
public enum FloorPlanSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a floor plan segment and its current status.</summary>
public sealed class FloorPlanSegment(string code, string title, double ratio, bool approved, List<string> tags, int retries, double discount, FloorPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public bool Approved { get; set; } = approved;
    public List<string> Tags { get; set; } = tags;
    public int Retries { get; set; } = retries;
    public double Discount { get; set; } = discount;
    public FloorPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan segment in status idle.</summary>
    public static FloorPlanSegment NewFloorPlanSegment(string code, string title)
    {
        return new FloorPlanSegment(
            code,
            title,
            0.0,
            false,
            new List<string>(),
            0,
            0.0,
            FloorPlanSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanSegmentStatus next)
    {
        switch (Status)
        {
            case FloorPlanSegmentStatus.Idle:
                return next == FloorPlanSegmentStatus.Stopped;
            case FloorPlanSegmentStatus.Warming:
                switch (next)
                {
                    case FloorPlanSegmentStatus.Ready:
                        return true;
                }
                return false;
            case FloorPlanSegmentStatus.Ready:
                return next == FloorPlanSegmentStatus.Draining || next == FloorPlanSegmentStatus.Stopped;
            case FloorPlanSegmentStatus.Draining:
                return next == FloorPlanSegmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FloorPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetExposure()
    {
        return Math.Round(Ratio - (double)Retries, 2);
    }

    public FloorPlanSegment Copy()
    {
        return new FloorPlanSegment(Code, Title, Ratio, Approved, Tags, Retries, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
