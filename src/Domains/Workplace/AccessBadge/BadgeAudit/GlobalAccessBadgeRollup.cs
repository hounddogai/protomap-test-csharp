namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global access badge rollup is in its life cycle.</summary>
public enum GlobalAccessBadgeRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One global access badge rollup, with what the planners need to act on it.</summary>
public sealed class GlobalAccessBadgeRollup(string code, string name, bool active, int sequence, double minimum, int graceDays, double cost, double ceiling, GlobalAccessBadgeRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int Sequence { get; set; } = sequence;
    public double Minimum { get; set; } = minimum;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public double Ceiling { get; set; } = ceiling;
    public GlobalAccessBadgeRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global access badge rollup in status created.</summary>
    public static GlobalAccessBadgeRollup NewGlobalAccessBadgeRollup(string code, string name)
    {
        return new GlobalAccessBadgeRollup(
            code,
            name,
            false,
            0,
            0.0,
            0,
            0.0,
            0.0,
            GlobalAccessBadgeRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAccessBadgeRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAccessBadgeRollupStatus next)
    {
        switch (Status)
        {
            case GlobalAccessBadgeRollupStatus.Created:
                return next == GlobalAccessBadgeRollupStatus.Published || next == GlobalAccessBadgeRollupStatus.Retired;
            case GlobalAccessBadgeRollupStatus.Validated:
                return next == GlobalAccessBadgeRollupStatus.Published || next == GlobalAccessBadgeRollupStatus.Retired;
            case GlobalAccessBadgeRollupStatus.Published:
                return next == GlobalAccessBadgeRollupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalAccessBadgeRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global access badge rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetTotal()
    {
        return Math.Round(Cost * Ceiling, 2);
    }

    public GlobalAccessBadgeRollup Copy()
    {
        return new GlobalAccessBadgeRollup(Code, Name, Active, Sequence, Minimum, GraceDays, Cost, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
