namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched tax form ledger is in its life cycle.</summary>
public enum BatchedTaxFormLedgerStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a batched tax form ledger and its current status.</summary>
public sealed class BatchedTaxFormLedger(string code, string name, double minimum, string costCenter, string unit, int updatedAt, double margin, BatchedTaxFormLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string CostCenter { get; set; } = costCenter;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Margin { get; set; } = margin;
    public BatchedTaxFormLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched tax form ledger in status idle.</summary>
    public static BatchedTaxFormLedger NewBatchedTaxFormLedger(string code, string name)
    {
        return new BatchedTaxFormLedger(code, name, 0.0, "", "", 0, 0.0, BatchedTaxFormLedgerStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTaxFormLedgerStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTaxFormLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedTaxFormLedgerStatus.Idle:
                switch (next)
                {
                    case BatchedTaxFormLedgerStatus.Warming:
                    case BatchedTaxFormLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTaxFormLedgerStatus.Warming:
                switch (next)
                {
                    case BatchedTaxFormLedgerStatus.Ready:
                        return true;
                }
                return false;
            case BatchedTaxFormLedgerStatus.Ready:
                switch (next)
                {
                    case BatchedTaxFormLedgerStatus.Draining:
                    case BatchedTaxFormLedgerStatus.Stopped:
                        return true;
                }
                return false;
            case BatchedTaxFormLedgerStatus.Draining:
                switch (next)
                {
                    case BatchedTaxFormLedgerStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTaxFormLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched tax form ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched tax form ledger"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)UpdatedAt + Margin, 2);
    }

    public BatchedTaxFormLedger Copy()
    {
        return new BatchedTaxFormLedger(Code, Name, Minimum, CostCenter, Unit, UpdatedAt, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
