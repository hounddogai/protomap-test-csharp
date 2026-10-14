namespace Acme.Workforce.Contractors.Offboarding.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding rule is in its life cycle.</summary>
public enum OffboardingRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A offboarding rule. Its code is unique within a tenant.</summary>
public sealed class OffboardingRule(string code, string label, string ownerId, int quantity, double discount, OffboardingRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public double Discount { get; set; } = discount;
    public OffboardingRuleStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding rule in status requested.</summary>
    public static OffboardingRule NewOffboardingRule(string code, string label)
    {
        return new OffboardingRule(code, label, "", 0, 0.0, OffboardingRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingRuleStatus next)
    {
        switch (Status)
        {
            case OffboardingRuleStatus.Requested:
                return next == OffboardingRuleStatus.Granted || next == OffboardingRuleStatus.Expired;
            case OffboardingRuleStatus.Granted:
                return next == OffboardingRuleStatus.Revoked || next == OffboardingRuleStatus.Expired;
            case OffboardingRuleStatus.Revoked:
                switch (next)
                {
                    case OffboardingRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OffboardingRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"offboarding rule"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Discount * (double)Quantity, 2);
    }

    public OffboardingRule Copy()
    {
        return new OffboardingRule(Code, Label, OwnerId, Quantity, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
