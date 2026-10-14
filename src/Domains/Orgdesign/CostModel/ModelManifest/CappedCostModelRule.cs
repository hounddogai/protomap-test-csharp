namespace Acme.Workforce.Orgdesign.CostModel.ModelManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped cost model rule is in its life cycle.</summary>
public enum CappedCostModelRuleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped cost model rule as the cost model team keeps it.</summary>
public sealed class CappedCostModelRule(string code, string name, string category, string timezone, double discount, bool visible, double fte, string costCenter, double ratio, string country, CappedCostModelRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public string Timezone { get; set; } = timezone;
    public double Discount { get; set; } = discount;
    public bool Visible { get; set; } = visible;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public double Ratio { get; set; } = ratio;
    public string Country { get; set; } = country;
    public CappedCostModelRuleStatus Status { get; set; } = status;

    /// <summary>Starts a capped cost model rule in status open.</summary>
    public static CappedCostModelRule NewCappedCostModelRule(string code, string name)
    {
        return new CappedCostModelRule(
            code,
            name,
            "",
            "",
            0.0,
            false,
            0.0,
            "",
            0.0,
            "",
            CappedCostModelRuleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCostModelRuleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCostModelRuleStatus next)
    {
        switch (Status)
        {
            case CappedCostModelRuleStatus.Open:
                return next == CappedCostModelRuleStatus.Reopened;
            case CappedCostModelRuleStatus.InReview:
                switch (next)
                {
                    case CappedCostModelRuleStatus.Reopened:
                        return true;
                }
                return false;
            case CappedCostModelRuleStatus.Resolved:
                switch (next)
                {
                    case CappedCostModelRuleStatus.Reopened:
                    case CappedCostModelRuleStatus.Archived:
                        return true;
                }
                return false;
            case CappedCostModelRuleStatus.Reopened:
                switch (next)
                {
                    case CappedCostModelRuleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCostModelRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped cost model rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Fte + Ratio, 2);
    }

    public CappedCostModelRule Copy()
    {
        return new CappedCostModelRule(
            Code,
            Name,
            Category,
            Timezone,
            Discount,
            Visible,
            Fte,
            CostCenter,
            Ratio,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
