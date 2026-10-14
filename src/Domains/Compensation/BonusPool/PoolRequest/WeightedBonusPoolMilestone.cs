namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool milestone is in its life cycle.</summary>
public enum WeightedBonusPoolMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted bonus pool milestone. Its code is unique within a tenant.</summary>
public sealed class WeightedBonusPoolMilestone(string code, string label, double rate, string phone, string summary, WeightedBonusPoolMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public WeightedBonusPoolMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool milestone in status planned.</summary>
    public static WeightedBonusPoolMilestone NewWeightedBonusPoolMilestone(string code, string label)
    {
        return new WeightedBonusPoolMilestone(code, label, 0.0, "", "", WeightedBonusPoolMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolMilestoneStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolMilestoneStatus.Planned:
                return next == WeightedBonusPoolMilestoneStatus.Scheduled || next == WeightedBonusPoolMilestoneStatus.Cancelled;
            case WeightedBonusPoolMilestoneStatus.Scheduled:
                switch (next)
                {
                    case WeightedBonusPoolMilestoneStatus.Completed:
                        return true;
                }
                return false;
            case WeightedBonusPoolMilestoneStatus.InProgress:
                return next == WeightedBonusPoolMilestoneStatus.Completed || next == WeightedBonusPoolMilestoneStatus.Cancelled;
            case WeightedBonusPoolMilestoneStatus.Completed:
                return next == WeightedBonusPoolMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted bonus pool milestone"} {Code}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public WeightedBonusPoolMilestone Copy()
    {
        return new WeightedBonusPoolMilestone(Code, Label, Rate, Phone, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
