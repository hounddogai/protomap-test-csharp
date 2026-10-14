namespace Acme.Workforce.Absence.Policy.PolicyRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy rule is in its life cycle.</summary>
public enum TieredPolicyRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered policy rule as the policy team keeps it.</summary>
public sealed class TieredPolicyRule(string code, string name, int leaveDays, string segment, bool visible, int batchSize, TieredPolicyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string Segment { get; set; } = segment;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public TieredPolicyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy rule in status created.</summary>
    public static TieredPolicyRule NewTieredPolicyRule(string code, string name)
    {
        return new TieredPolicyRule(code, name, 0, "", false, 0, TieredPolicyRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyRuleStatus next)
    {
        switch (Status)
        {
            case TieredPolicyRuleStatus.Created:
                switch (next)
                {
                    case TieredPolicyRuleStatus.Published:
                        return true;
                }
                return false;
            case TieredPolicyRuleStatus.Validated:
                return next == TieredPolicyRuleStatus.Retired;
            case TieredPolicyRuleStatus.Published:
                return next == TieredPolicyRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredPolicyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tiered policy rule"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays * (double)BatchSize, 2);
    }

    public TieredPolicyRule Copy()
    {
        return new TieredPolicyRule(Code, Name, LeaveDays, Segment, Visible, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
