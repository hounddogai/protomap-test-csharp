namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling bonus pool profile is in its life cycle.</summary>
public enum RollingBonusPoolProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One rolling bonus pool profile, with what the planners need to act on it.</summary>
public sealed class RollingBonusPoolProfile(string code, string name, string summary, string currency, string skill, string label, string grade, bool approved, double amount, RollingBonusPoolProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public bool Approved { get; set; } = approved;
    public double Amount { get; set; } = amount;
    public RollingBonusPoolProfileStatus Status { get; set; } = status;

    /// <summary>Starts a rolling bonus pool profile in status draft.</summary>
    public static RollingBonusPoolProfile NewRollingBonusPoolProfile(string code, string name)
    {
        return new RollingBonusPoolProfile(
            code,
            name,
            "",
            "",
            "",
            "",
            "",
            false,
            0.0,
            RollingBonusPoolProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingBonusPoolProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingBonusPoolProfileStatus next)
    {
        switch (Status)
        {
            case RollingBonusPoolProfileStatus.Draft:
                return next == RollingBonusPoolProfileStatus.Active;
            case RollingBonusPoolProfileStatus.Pending:
                return next == RollingBonusPoolProfileStatus.Suspended || next == RollingBonusPoolProfileStatus.Closed;
            case RollingBonusPoolProfileStatus.Active:
                return next == RollingBonusPoolProfileStatus.Suspended || next == RollingBonusPoolProfileStatus.Closed;
            case RollingBonusPoolProfileStatus.Suspended:
                return next == RollingBonusPoolProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RollingBonusPoolProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling bonus pool profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public RollingBonusPoolProfile Copy()
    {
        return new RollingBonusPoolProfile(
            Code,
            Name,
            Summary,
            Currency,
            Skill,
            Label,
            Grade,
            Approved,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
