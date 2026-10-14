namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility rule is in its life cycle.</summary>
public enum MobilityRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A mobility rule. Its code is unique within a tenant.</summary>
public sealed class MobilityRule(string code, string name, double amount, int tenureMonths, string summary, double weight, string category, string reviewer, double ceiling, string description, MobilityRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public MobilityRuleStatus Status { get; set; } = status;

    /// <summary>Starts a mobility rule in status requested.</summary>
    public static MobilityRule NewMobilityRule(string code, string name)
    {
        return new MobilityRule(code, name, 0.0, 0, "", 0.0, "", "", 0.0, "", MobilityRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityRuleStatus next)
    {
        switch (Status)
        {
            case MobilityRuleStatus.Requested:
                return next == MobilityRuleStatus.Granted || next == MobilityRuleStatus.Revoked;
            case MobilityRuleStatus.Granted:
                return next == MobilityRuleStatus.Revoked;
            case MobilityRuleStatus.Revoked:
                switch (next)
                {
                    case MobilityRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MobilityRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"mobility rule"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)TenureMonths + Ceiling, 2);
    }

    public MobilityRule Copy()
    {
        return new MobilityRule(
            Code,
            Name,
            Amount,
            TenureMonths,
            Summary,
            Weight,
            Category,
            Reviewer,
            Ceiling,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
