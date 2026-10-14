namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty rule is in its life cycle.</summary>
public enum WarrantyRuleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A warranty rule. Its code is unique within a tenant.</summary>
public sealed class WarrantyRule(string code, string label, double floorValue, double score, int limit, WarrantyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public double Score { get; set; } = score;
    public int Limit { get; set; } = limit;
    public WarrantyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a warranty rule in status proposed.</summary>
    public static WarrantyRule NewWarrantyRule(string code, string label)
    {
        return new WarrantyRule(code, label, 0.0, 0.0, 0, WarrantyRuleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyRuleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyRuleStatus next)
    {
        switch (Status)
        {
            case WarrantyRuleStatus.Proposed:
                switch (next)
                {
                    case WarrantyRuleStatus.Withdrawn:
                        return true;
                }
                return false;
            case WarrantyRuleStatus.Approved:
                return next == WarrantyRuleStatus.Withdrawn;
            case WarrantyRuleStatus.Rejected:
                return next == WarrantyRuleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(WarrantyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue * (double)Limit, 2);
    }

    public WarrantyRule Copy()
    {
        return new WarrantyRule(Code, Label, FloorValue, Score, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
