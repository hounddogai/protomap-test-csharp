namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pool milestone is in its life cycle.</summary>
public enum ScheduledPoolMilestoneStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled pool milestone. Its code is unique within a tenant.</summary>
public sealed class ScheduledPoolMilestone(string code, string label, string name, int quantity, double margin, string unit, ScheduledPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public double Margin { get; set; } = margin;
    public string Unit { get; set; } = unit;
    public ScheduledPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pool milestone in status new.</summary>
    public static ScheduledPoolMilestone NewScheduledPoolMilestone(string code, string label)
    {
        return new ScheduledPoolMilestone(code, label, "", 0, 0.0, "", ScheduledPoolMilestoneStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPoolMilestoneStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case ScheduledPoolMilestoneStatus.New:
                return next == ScheduledPoolMilestoneStatus.Running || next == ScheduledPoolMilestoneStatus.Failed;
            case ScheduledPoolMilestoneStatus.Queued:
                return next == ScheduledPoolMilestoneStatus.Running;
            case ScheduledPoolMilestoneStatus.Running:
                switch (next)
                {
                    case ScheduledPoolMilestoneStatus.Done:
                    case ScheduledPoolMilestoneStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPoolMilestoneStatus.Done:
                return next == ScheduledPoolMilestoneStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin * (double)Quantity, 2);
    }

    public ScheduledPoolMilestone Copy()
    {
        return new ScheduledPoolMilestone(Code, Label, Name, Quantity, Margin, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
