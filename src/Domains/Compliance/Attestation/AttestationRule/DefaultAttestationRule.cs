namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default attestation rule is in its life cycle.</summary>
public enum DefaultAttestationRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One default attestation rule, with what the planners need to act on it.</summary>
public sealed class DefaultAttestationRule(string code, string name, string category, string reviewer, int expiresAt, double budget, string salaryBand, string currency, DefaultAttestationRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public string SalaryBand { get; set; } = salaryBand;
    public string Currency { get; set; } = currency;
    public DefaultAttestationRuleStatus Status { get; set; } = status;

    /// <summary>Starts a default attestation rule in status planned.</summary>
    public static DefaultAttestationRule NewDefaultAttestationRule(string code, string name)
    {
        return new DefaultAttestationRule(code, name, "", "", 0, 0.0, "", "", DefaultAttestationRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAttestationRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAttestationRuleStatus next)
    {
        switch (Status)
        {
            case DefaultAttestationRuleStatus.Planned:
                switch (next)
                {
                    case DefaultAttestationRuleStatus.Scheduled:
                        return true;
                }
                return false;
            case DefaultAttestationRuleStatus.Scheduled:
                return next == DefaultAttestationRuleStatus.Cancelled;
            case DefaultAttestationRuleStatus.InProgress:
                switch (next)
                {
                    case DefaultAttestationRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultAttestationRuleStatus.Completed:
                switch (next)
                {
                    case DefaultAttestationRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAttestationRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default attestation rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Budget - (double)ExpiresAt, 2);
    }

    public DefaultAttestationRule Copy()
    {
        return new DefaultAttestationRule(
            Code,
            Name,
            Category,
            Reviewer,
            ExpiresAt,
            Budget,
            SalaryBand,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
