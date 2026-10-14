namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global access badge cycle is in its life cycle.</summary>
public enum GlobalAccessBadgeCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One global access badge cycle, with what the planners need to act on it.</summary>
public sealed class GlobalAccessBadgeCycle(string code, string title, string currency, string team, string timezone, bool active, string skill, double rate, string location, double cost, GlobalAccessBadgeCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public string Timezone { get; set; } = timezone;
    public bool Active { get; set; } = active;
    public string Skill { get; set; } = skill;
    public double Rate { get; set; } = rate;
    public string Location { get; set; } = location;
    public double Cost { get; set; } = cost;
    public GlobalAccessBadgeCycleStatus Status { get; set; } = status;

    /// <summary>Starts a global access badge cycle in status proposed.</summary>
    public static GlobalAccessBadgeCycle NewGlobalAccessBadgeCycle(string code, string title)
    {
        return new GlobalAccessBadgeCycle(
            code,
            title,
            "",
            "",
            "",
            false,
            "",
            0.0,
            "",
            0.0,
            GlobalAccessBadgeCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAccessBadgeCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAccessBadgeCycleStatus next)
    {
        switch (Status)
        {
            case GlobalAccessBadgeCycleStatus.Proposed:
                return next == GlobalAccessBadgeCycleStatus.Rejected || next == GlobalAccessBadgeCycleStatus.Withdrawn;
            case GlobalAccessBadgeCycleStatus.Approved:
                return next == GlobalAccessBadgeCycleStatus.Withdrawn;
            case GlobalAccessBadgeCycleStatus.Rejected:
                switch (next)
                {
                    case GlobalAccessBadgeCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAccessBadgeCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global access badge cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Cost - Rate, 2);
    }

    public GlobalAccessBadgeCycle Copy()
    {
        return new GlobalAccessBadgeCycle(
            Code,
            Title,
            Currency,
            Team,
            Timezone,
            Active,
            Skill,
            Rate,
            Location,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
