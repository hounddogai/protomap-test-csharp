namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched warranty ledger is in its life cycle.</summary>
public enum BatchedWarrantyLedgerStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A batched warranty ledger. Its code is unique within a tenant.</summary>
public sealed class BatchedWarrantyLedger(string code, string label, double minimum, string phone, double cost, string location, double hours, double weight, double floorValue, string unit, BatchedWarrantyLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public double Cost { get; set; } = cost;
    public string Location { get; set; } = location;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public string Unit { get; set; } = unit;
    public BatchedWarrantyLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched warranty ledger in status requested.</summary>
    public static BatchedWarrantyLedger NewBatchedWarrantyLedger(string code, string label)
    {
        return new BatchedWarrantyLedger(
            code,
            label,
            0.0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            BatchedWarrantyLedgerStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedWarrantyLedgerStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedWarrantyLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedWarrantyLedgerStatus.Requested:
                return next == BatchedWarrantyLedgerStatus.Granted;
            case BatchedWarrantyLedgerStatus.Granted:
                return next == BatchedWarrantyLedgerStatus.Expired;
            case BatchedWarrantyLedgerStatus.Revoked:
                return next == BatchedWarrantyLedgerStatus.Expired;
        }
        return false;
    }

    public void MoveTo(BatchedWarrantyLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched warranty ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"batched warranty ledger"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(FloorValue + Hours, 2);
    }

    public BatchedWarrantyLedger Copy()
    {
        return new BatchedWarrantyLedger(
            Code,
            Label,
            Minimum,
            Phone,
            Cost,
            Location,
            Hours,
            Weight,
            FloorValue,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
