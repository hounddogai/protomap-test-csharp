namespace Acme.Workforce.Assets.Warranty.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional warranty ledger is in its life cycle.</summary>
public enum RegionalWarrantyLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional warranty ledger. Its code is unique within a tenant.</summary>
public sealed class RegionalWarrantyLedger(string code, string title, string costCenter, int windowDays, double amount, double rate, string name, int graceDays, string country, RegionalWarrantyLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public int WindowDays { get; set; } = windowDays;
    public double Amount { get; set; } = amount;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public string Country { get; set; } = country;
    public RegionalWarrantyLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a regional warranty ledger in status new.</summary>
    public static RegionalWarrantyLedger NewRegionalWarrantyLedger(string code, string title)
    {
        return new RegionalWarrantyLedger(code, title, "", 0, 0.0, 0.0, "", 0, "", RegionalWarrantyLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalWarrantyLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalWarrantyLedgerStatus next)
    {
        switch (Status)
        {
            case RegionalWarrantyLedgerStatus.New:
                switch (next)
                {
                    case RegionalWarrantyLedgerStatus.Queued:
                        return true;
                }
                return false;
            case RegionalWarrantyLedgerStatus.Queued:
                return next == RegionalWarrantyLedgerStatus.Running || next == RegionalWarrantyLedgerStatus.Failed;
            case RegionalWarrantyLedgerStatus.Running:
                switch (next)
                {
                    case RegionalWarrantyLedgerStatus.Done:
                    case RegionalWarrantyLedgerStatus.Failed:
                        return true;
                }
                return false;
            case RegionalWarrantyLedgerStatus.Done:
                switch (next)
                {
                    case RegionalWarrantyLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalWarrantyLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional warranty ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Amount + (double)GraceDays, 2);
    }

    public RegionalWarrantyLedger Copy()
    {
        return new RegionalWarrantyLedger(
            Code,
            Title,
            CostCenter,
            WindowDays,
            Amount,
            Rate,
            Name,
            GraceDays,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
