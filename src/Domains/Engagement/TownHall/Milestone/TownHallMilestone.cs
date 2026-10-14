namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall milestone is in its life cycle.</summary>
public enum TownHallMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a town hall milestone and its current status.</summary>
public sealed class TownHallMilestone(string code, string label, double weight, string category, double minimum, int batchSize, double maximum, string notes, double fte, TownHallMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public double Minimum { get; set; } = minimum;
    public int BatchSize { get; set; } = batchSize;
    public double Maximum { get; set; } = maximum;
    public string Notes { get; set; } = notes;
    public double Fte { get; set; } = fte;
    public TownHallMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a town hall milestone in status idle.</summary>
    public static TownHallMilestone NewTownHallMilestone(string code, string label)
    {
        return new TownHallMilestone(code, label, 0.0, "", 0.0, 0, 0.0, "", 0.0, TownHallMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallMilestoneStatus next)
    {
        switch (Status)
        {
            case TownHallMilestoneStatus.Idle:
                return next == TownHallMilestoneStatus.Stopped;
            case TownHallMilestoneStatus.Warming:
                return next == TownHallMilestoneStatus.Ready || next == TownHallMilestoneStatus.Draining;
            case TownHallMilestoneStatus.Ready:
                return next == TownHallMilestoneStatus.Draining || next == TownHallMilestoneStatus.Stopped;
            case TownHallMilestoneStatus.Draining:
                return next == TownHallMilestoneStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(TownHallMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Minimum * (double)BatchSize, 2);
    }

    public TownHallMilestone Copy()
    {
        return new TownHallMilestone(Code, Label, Weight, Category, Minimum, BatchSize, Maximum, Notes, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
