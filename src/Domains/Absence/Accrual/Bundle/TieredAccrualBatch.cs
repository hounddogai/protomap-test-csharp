namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered accrual batch is in its life cycle.</summary>
public enum TieredAccrualBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tiered accrual batch as the accrual team keeps it.</summary>
public sealed class TieredAccrualBatch(string code, string label, bool visible, int leaveDays, string costCenter, double weight, bool pinned, int batchSize, TieredAccrualBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public int LeaveDays { get; set; } = leaveDays;
    public string CostCenter { get; set; } = costCenter;
    public double Weight { get; set; } = weight;
    public bool Pinned { get; set; } = pinned;
    public int BatchSize { get; set; } = batchSize;
    public TieredAccrualBatchStatus Status { get; set; } = status;

    /// <summary>Starts a tiered accrual batch in status created.</summary>
    public static TieredAccrualBatch NewTieredAccrualBatch(string code, string label)
    {
        return new TieredAccrualBatch(code, label, false, 0, "", 0.0, false, 0, TieredAccrualBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccrualBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccrualBatchStatus next)
    {
        switch (Status)
        {
            case TieredAccrualBatchStatus.Created:
                switch (next)
                {
                    case TieredAccrualBatchStatus.Published:
                        return true;
                }
                return false;
            case TieredAccrualBatchStatus.Validated:
                return next == TieredAccrualBatchStatus.Published || next == TieredAccrualBatchStatus.Retired;
            case TieredAccrualBatchStatus.Published:
                switch (next)
                {
                    case TieredAccrualBatchStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredAccrualBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered accrual batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Weight - (double)LeaveDays, 2);
    }

    public TieredAccrualBatch Copy()
    {
        return new TieredAccrualBatch(
            Code,
            Label,
            Visible,
            LeaveDays,
            CostCenter,
            Weight,
            Pinned,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
