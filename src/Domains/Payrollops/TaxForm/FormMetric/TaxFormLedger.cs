namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form ledger is in its life cycle.</summary>
public enum TaxFormLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One tax form ledger, with what the planners need to act on it.</summary>
public sealed class TaxFormLedger(string code, string name, int updatedAt, double hours, int startsAt, string location, TaxFormLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Hours { get; set; } = hours;
    public int StartsAt { get; set; } = startsAt;
    public string Location { get; set; } = location;
    public TaxFormLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a tax form ledger in status proposed.</summary>
    public static TaxFormLedger NewTaxFormLedger(string code, string name)
    {
        return new TaxFormLedger(code, name, 0, 0.0, 0, "", TaxFormLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormLedgerStatus next)
    {
        switch (Status)
        {
            case TaxFormLedgerStatus.Proposed:
                switch (next)
                {
                    case TaxFormLedgerStatus.Approved:
                        return true;
                }
                return false;
            case TaxFormLedgerStatus.Approved:
                return next == TaxFormLedgerStatus.Rejected;
            case TaxFormLedgerStatus.Rejected:
                switch (next)
                {
                    case TaxFormLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)StartsAt + (double)UpdatedAt, 2);
    }

    public TaxFormLedger Copy()
    {
        return new TaxFormLedger(Code, Name, UpdatedAt, Hours, StartsAt, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
