namespace Acme.Workforce.Talent.Nomination.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered nomination rule is in its life cycle.</summary>
public enum TieredNominationRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tiered nomination rule. Its code is unique within a tenant.</summary>
public sealed class TieredNominationRule(string code, string name, string unit, string salaryBand, int createdAt, double floorValue, string locale, bool archived, double maximum, TieredNominationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public int CreatedAt { get; set; } = createdAt;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public bool Archived { get; set; } = archived;
    public double Maximum { get; set; } = maximum;
    public TieredNominationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered nomination rule in status planned.</summary>
    public static TieredNominationRule NewTieredNominationRule(string code, string name)
    {
        return new TieredNominationRule(
            code,
            name,
            "",
            "",
            0,
            0.0,
            "",
            false,
            0.0,
            TieredNominationRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredNominationRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredNominationRuleStatus next)
    {
        switch (Status)
        {
            case TieredNominationRuleStatus.Planned:
                switch (next)
                {
                    case TieredNominationRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredNominationRuleStatus.Scheduled:
                return next == TieredNominationRuleStatus.Cancelled;
            case TieredNominationRuleStatus.InProgress:
                switch (next)
                {
                    case TieredNominationRuleStatus.Completed:
                    case TieredNominationRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredNominationRuleStatus.Completed:
                return next == TieredNominationRuleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TieredNominationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered nomination rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue * Maximum, 2);
    }

    public TieredNominationRule Copy()
    {
        return new TieredNominationRule(
            Code,
            Name,
            Unit,
            SalaryBand,
            CreatedAt,
            FloorValue,
            Locale,
            Archived,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
