namespace Acme.Workforce.Engagement.ActionPlan.PlanSegment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback action plan segment is in its life cycle.</summary>
public enum FallbackActionPlanSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback action plan segment, with what the planners need to act on it.</summary>
public sealed class FallbackActionPlanSegment(string code, string name, double fte, double threshold, string segment, int batchSize, string department, double floorValue, string grade, FallbackActionPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public double Threshold { get; set; } = threshold;
    public string Segment { get; set; } = segment;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string Grade { get; set; } = grade;
    public FallbackActionPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback action plan segment in status requested.</summary>
    public static FallbackActionPlanSegment NewFallbackActionPlanSegment(string code, string name)
    {
        return new FallbackActionPlanSegment(
            code,
            name,
            0.0,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            FallbackActionPlanSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackActionPlanSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackActionPlanSegmentStatus next)
    {
        switch (Status)
        {
            case FallbackActionPlanSegmentStatus.Requested:
                return next == FallbackActionPlanSegmentStatus.Revoked || next == FallbackActionPlanSegmentStatus.Expired;
            case FallbackActionPlanSegmentStatus.Granted:
                switch (next)
                {
                    case FallbackActionPlanSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackActionPlanSegmentStatus.Revoked:
                switch (next)
                {
                    case FallbackActionPlanSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackActionPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback action plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Threshold * (double)BatchSize, 2);
    }

    public FallbackActionPlanSegment Copy()
    {
        return new FallbackActionPlanSegment(
            Code,
            Name,
            Fte,
            Threshold,
            Segment,
            BatchSize,
            Department,
            FloorValue,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
