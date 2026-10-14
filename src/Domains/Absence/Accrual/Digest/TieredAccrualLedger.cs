namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered accrual ledger is in its life cycle.</summary>
public enum TieredAccrualLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One tiered accrual ledger, with what the planners need to act on it.</summary>
public sealed class TieredAccrualLedger(string code, string name, string grade, string locale, int quantity, bool approved, string currency, TieredAccrualLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public TieredAccrualLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tiered accrual ledger in status new.</summary>
    public static TieredAccrualLedger NewTieredAccrualLedger(string code, string name)
    {
        return new TieredAccrualLedger(code, name, "", "", 0, false, "", TieredAccrualLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredAccrualLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredAccrualLedgerStatus next)
    {
        switch (Status)
        {
            case TieredAccrualLedgerStatus.New:
                switch (next)
                {
                    case TieredAccrualLedgerStatus.Queued:
                    case TieredAccrualLedgerStatus.Done:
                        return true;
                }
                return false;
            case TieredAccrualLedgerStatus.Queued:
                return next == TieredAccrualLedgerStatus.Running || next == TieredAccrualLedgerStatus.Failed;
            case TieredAccrualLedgerStatus.Running:
                return next == TieredAccrualLedgerStatus.Done;
            case TieredAccrualLedgerStatus.Done:
                return next == TieredAccrualLedgerStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TieredAccrualLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered accrual ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public TieredAccrualLedger Copy()
    {
        return new TieredAccrualLedger(Code, Name, Grade, Locale, Quantity, Approved, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
