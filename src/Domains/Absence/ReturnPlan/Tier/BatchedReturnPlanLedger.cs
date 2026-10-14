namespace Acme.Workforce.Absence.ReturnPlan.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched return plan ledger is in its life cycle.</summary>
public enum BatchedReturnPlanLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a batched return plan ledger and its current status.</summary>
public sealed class BatchedReturnPlanLedger(string code, string name, int windowDays, double discount, double maximum, List<string> tags, int tenureMonths, string description, int quantity, string unit, double ceiling, BatchedReturnPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public double Discount { get; set; } = discount;
    public double Maximum { get; set; } = maximum;
    public List<string> Tags { get; set; } = tags;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public BatchedReturnPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched return plan ledger in status draft.</summary>
    public static BatchedReturnPlanLedger NewBatchedReturnPlanLedger(string code, string name)
    {
        return new BatchedReturnPlanLedger(
            code,
            name,
            0,
            0.0,
            0.0,
            new List<string>(),
            0,
            "",
            0,
            "",
            0.0,
            BatchedReturnPlanLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedReturnPlanLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedReturnPlanLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedReturnPlanLedgerStatus.Draft:
                switch (next)
                {
                    case BatchedReturnPlanLedgerStatus.Suspended:
                    case BatchedReturnPlanLedgerStatus.Closed:
                        return true;
                }
                return false;
            case BatchedReturnPlanLedgerStatus.Pending:
                switch (next)
                {
                    case BatchedReturnPlanLedgerStatus.Closed:
                        return true;
                }
                return false;
            case BatchedReturnPlanLedgerStatus.Active:
                return next == BatchedReturnPlanLedgerStatus.Closed;
            case BatchedReturnPlanLedgerStatus.Suspended:
                switch (next)
                {
                    case BatchedReturnPlanLedgerStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedReturnPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched return plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Quantity - (double)WindowDays, 2);
    }

    public BatchedReturnPlanLedger Copy()
    {
        return new BatchedReturnPlanLedger(
            Code,
            Name,
            WindowDays,
            Discount,
            Maximum,
            Tags,
            TenureMonths,
            Description,
            Quantity,
            Unit,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
