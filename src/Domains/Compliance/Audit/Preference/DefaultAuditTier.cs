namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default audit tier is in its life cycle.</summary>
public enum DefaultAuditTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One default audit tier, with what the planners need to act on it.</summary>
public sealed class DefaultAuditTier(string code, string label, int createdAt, string skill, int graceDays, double cost, double rate, double ceiling, string unit, string category, int startsAt, DefaultAuditTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public string Skill { get; set; } = skill;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public string Unit { get; set; } = unit;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public DefaultAuditTierStatus Status { get; set; } = status;

    /// <summary>Starts a default audit tier in status draft.</summary>
    public static DefaultAuditTier NewDefaultAuditTier(string code, string label)
    {
        return new DefaultAuditTier(code, label, 0, "", 0, 0.0, 0.0, 0.0, "", "", 0, DefaultAuditTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAuditTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAuditTierStatus next)
    {
        switch (Status)
        {
            case DefaultAuditTierStatus.Draft:
                return next == DefaultAuditTierStatus.Active;
            case DefaultAuditTierStatus.Pending:
                switch (next)
                {
                    case DefaultAuditTierStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultAuditTierStatus.Active:
                return next == DefaultAuditTierStatus.Closed;
            case DefaultAuditTierStatus.Suspended:
                return next == DefaultAuditTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultAuditTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default audit tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"default audit tier"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Ceiling + Cost, 2);
    }

    public DefaultAuditTier Copy()
    {
        return new DefaultAuditTier(
            Code,
            Label,
            CreatedAt,
            Skill,
            GraceDays,
            Cost,
            Rate,
            Ceiling,
            Unit,
            Category,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
