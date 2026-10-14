namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default floor plan segment is in its life cycle.</summary>
public enum DefaultFloorPlanSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One default floor plan segment, with what the planners need to act on it.</summary>
public sealed class DefaultFloorPlanSegment(string code, string name, double score, int leaveDays, string managerId, string summary, double amount, bool visible, double minimum, double ratio, string contactEmail, DefaultFloorPlanSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Score { get; set; } = score;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public string Summary { get; set; } = summary;
    public double Amount { get; set; } = amount;
    public bool Visible { get; set; } = visible;
    public double Minimum { get; set; } = minimum;
    public double Ratio { get; set; } = ratio;
    public string ContactEmail { get; set; } = contactEmail;
    public DefaultFloorPlanSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a default floor plan segment in status new.</summary>
    public static DefaultFloorPlanSegment NewDefaultFloorPlanSegment(string code, string name)
    {
        return new DefaultFloorPlanSegment(
            code,
            name,
            0.0,
            0,
            "",
            "",
            0.0,
            false,
            0.0,
            0.0,
            "",
            DefaultFloorPlanSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultFloorPlanSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultFloorPlanSegmentStatus next)
    {
        switch (Status)
        {
            case DefaultFloorPlanSegmentStatus.New:
                return next == DefaultFloorPlanSegmentStatus.Running || next == DefaultFloorPlanSegmentStatus.Failed;
            case DefaultFloorPlanSegmentStatus.Queued:
                return next == DefaultFloorPlanSegmentStatus.Done;
            case DefaultFloorPlanSegmentStatus.Running:
                return next == DefaultFloorPlanSegmentStatus.Done;
            case DefaultFloorPlanSegmentStatus.Done:
                return next == DefaultFloorPlanSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(DefaultFloorPlanSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default floor plan segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Amount + (double)LeaveDays, 2);
    }

    public DefaultFloorPlanSegment Copy()
    {
        return new DefaultFloorPlanSegment(
            Code,
            Name,
            Score,
            LeaveDays,
            ManagerId,
            Summary,
            Amount,
            Visible,
            Minimum,
            Ratio,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
