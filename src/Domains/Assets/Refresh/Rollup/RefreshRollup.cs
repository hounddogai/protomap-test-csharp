namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh rollup is in its life cycle.</summary>
public enum RefreshRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A refresh rollup as the refresh team keeps it.</summary>
public sealed class RefreshRollup(string code, string label, string skill, int sequence, double rate, string employeeId, double discount, string team, string description, RefreshRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public double Discount { get; set; } = discount;
    public string Team { get; set; } = team;
    public string Description { get; set; } = description;
    public RefreshRollupStatus Status { get; set; } = status;

    /// <summary>Starts a refresh rollup in status proposed.</summary>
    public static RefreshRollup NewRefreshRollup(string code, string label)
    {
        return new RefreshRollup(code, label, "", 0, 0.0, "", 0.0, "", "", RefreshRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshRollupStatus next)
    {
        switch (Status)
        {
            case RefreshRollupStatus.Proposed:
                return next == RefreshRollupStatus.Rejected || next == RefreshRollupStatus.Withdrawn;
            case RefreshRollupStatus.Approved:
                return next == RefreshRollupStatus.Rejected || next == RefreshRollupStatus.Withdrawn;
            case RefreshRollupStatus.Rejected:
                switch (next)
                {
                    case RefreshRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RefreshRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"refresh rollup"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Sequence - Discount, 2);
    }

    public RefreshRollup Copy()
    {
        return new RefreshRollup(
            Code,
            Label,
            Skill,
            Sequence,
            Rate,
            EmployeeId,
            Discount,
            Team,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
