namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool target is in its life cycle.</summary>
public enum PoolTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pool target, with what the planners need to act on it.</summary>
public sealed class PoolTarget(string code, string name, double cost, string location, string costCenter, string grade, int graceDays, string title, double discount, string skill, PoolTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public string CostCenter { get; set; } = costCenter;
    public string Grade { get; set; } = grade;
    public int GraceDays { get; set; } = graceDays;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public PoolTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pool target in status draft.</summary>
    public static PoolTarget NewPoolTarget(string code, string name)
    {
        return new PoolTarget(code, name, 0.0, "", "", "", 0, "", 0.0, "", PoolTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolTargetStatus next)
    {
        switch (Status)
        {
            case PoolTargetStatus.Draft:
                return next == PoolTargetStatus.Closed;
            case PoolTargetStatus.Pending:
                switch (next)
                {
                    case PoolTargetStatus.Active:
                    case PoolTargetStatus.Suspended:
                        return true;
                }
                return false;
            case PoolTargetStatus.Active:
                return next == PoolTargetStatus.Suspended;
            case PoolTargetStatus.Suspended:
                return next == PoolTargetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PoolTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Cost * Discount, 2);
    }

    public PoolTarget Copy()
    {
        return new PoolTarget(
            Code,
            Name,
            Cost,
            Location,
            CostCenter,
            Grade,
            GraceDays,
            Title,
            Discount,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
