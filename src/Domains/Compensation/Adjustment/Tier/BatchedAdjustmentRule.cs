namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched adjustment rule is in its life cycle.</summary>
public enum BatchedAdjustmentRuleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched adjustment rule as the adjustment team keeps it.</summary>
public sealed class BatchedAdjustmentRule(string code, string title, string team, string salaryBand, double budget, double maximum, string category, double cost, double threshold, BatchedAdjustmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public double Maximum { get; set; } = maximum;
    public string Category { get; set; } = category;
    public double Cost { get; set; } = cost;
    public double Threshold { get; set; } = threshold;
    public BatchedAdjustmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a batched adjustment rule in status requested.</summary>
    public static BatchedAdjustmentRule NewBatchedAdjustmentRule(string code, string title)
    {
        return new BatchedAdjustmentRule(
            code,
            title,
            "",
            "",
            0.0,
            0.0,
            "",
            0.0,
            0.0,
            BatchedAdjustmentRuleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAdjustmentRuleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAdjustmentRuleStatus next)
    {
        switch (Status)
        {
            case BatchedAdjustmentRuleStatus.Requested:
                return next == BatchedAdjustmentRuleStatus.Revoked;
            case BatchedAdjustmentRuleStatus.Granted:
                return next == BatchedAdjustmentRuleStatus.Revoked || next == BatchedAdjustmentRuleStatus.Expired;
            case BatchedAdjustmentRuleStatus.Revoked:
                switch (next)
                {
                    case BatchedAdjustmentRuleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAdjustmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched adjustment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * Cost, 2);
    }

    public BatchedAdjustmentRule Copy()
    {
        return new BatchedAdjustmentRule(
            Code,
            Title,
            Team,
            SalaryBand,
            Budget,
            Maximum,
            Category,
            Cost,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
