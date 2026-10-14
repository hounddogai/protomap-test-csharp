namespace Acme.Workforce.Compensation.BonusPool.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool rollup is in its life cycle.</summary>
public enum BonusPoolRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a bonus pool rollup and its current status.</summary>
public sealed class BonusPoolRollup(string code, string name, string label, int leaveDays, int priority, string ownerId, double ceiling, bool approved, string salaryBand, BonusPoolRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public int Priority { get; set; } = priority;
    public string OwnerId { get; set; } = ownerId;
    public double Ceiling { get; set; } = ceiling;
    public bool Approved { get; set; } = approved;
    public string SalaryBand { get; set; } = salaryBand;
    public BonusPoolRollupStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool rollup in status planned.</summary>
    public static BonusPoolRollup NewBonusPoolRollup(string code, string name)
    {
        return new BonusPoolRollup(code, name, "", 0, 0, "", 0.0, false, "", BonusPoolRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolRollupStatus next)
    {
        switch (Status)
        {
            case BonusPoolRollupStatus.Planned:
                switch (next)
                {
                    case BonusPoolRollupStatus.Completed:
                    case BonusPoolRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolRollupStatus.Scheduled:
                switch (next)
                {
                    case BonusPoolRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolRollupStatus.InProgress:
                return next == BonusPoolRollupStatus.Completed || next == BonusPoolRollupStatus.Cancelled;
            case BonusPoolRollupStatus.Completed:
                return next == BonusPoolRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BonusPoolRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool rollup"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays + (double)Priority, 2);
    }

    public BonusPoolRollup Copy()
    {
        return new BonusPoolRollup(
            Code,
            Name,
            Label,
            LeaveDays,
            Priority,
            OwnerId,
            Ceiling,
            Approved,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
