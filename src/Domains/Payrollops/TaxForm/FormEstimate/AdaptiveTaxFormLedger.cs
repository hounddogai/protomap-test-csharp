namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive tax form ledger is in its life cycle.</summary>
public enum AdaptiveTaxFormLedgerStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adaptive tax form ledger. Its code is unique within a tenant.</summary>
public sealed class AdaptiveTaxFormLedger(string code, string label, int durationMinutes, double minimum, string locale, AdaptiveTaxFormLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Minimum { get; set; } = minimum;
    public string Locale { get; set; } = locale;
    public AdaptiveTaxFormLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive tax form ledger in status draft.</summary>
    public static AdaptiveTaxFormLedger NewAdaptiveTaxFormLedger(string code, string label)
    {
        return new AdaptiveTaxFormLedger(code, label, 0, 0.0, "", AdaptiveTaxFormLedgerStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveTaxFormLedgerStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveTaxFormLedgerStatus next)
    {
        switch (Status)
        {
            case AdaptiveTaxFormLedgerStatus.Draft:
                switch (next)
                {
                    case AdaptiveTaxFormLedgerStatus.Pending:
                    case AdaptiveTaxFormLedgerStatus.Active:
                        return true;
                }
                return false;
            case AdaptiveTaxFormLedgerStatus.Pending:
                switch (next)
                {
                    case AdaptiveTaxFormLedgerStatus.Suspended:
                    case AdaptiveTaxFormLedgerStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveTaxFormLedgerStatus.Active:
                switch (next)
                {
                    case AdaptiveTaxFormLedgerStatus.Suspended:
                    case AdaptiveTaxFormLedgerStatus.Closed:
                        return true;
                }
                return false;
            case AdaptiveTaxFormLedgerStatus.Suspended:
                return next == AdaptiveTaxFormLedgerStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdaptiveTaxFormLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive tax form ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)DurationMinutes - Minimum, 2);
    }

    public AdaptiveTaxFormLedger Copy()
    {
        return new AdaptiveTaxFormLedger(Code, Label, DurationMinutes, Minimum, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
