namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form agreement is in its life cycle.</summary>
public enum TaxFormAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tax form agreement as the tax form team keeps it.</summary>
public sealed class TaxFormAgreement(string code, string title, string source, string description, string locale, string checksum, double threshold, TaxFormAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Description { get; set; } = description;
    public string Locale { get; set; } = locale;
    public string Checksum { get; set; } = checksum;
    public double Threshold { get; set; } = threshold;
    public TaxFormAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a tax form agreement in status draft.</summary>
    public static TaxFormAgreement NewTaxFormAgreement(string code, string title)
    {
        return new TaxFormAgreement(code, title, "", "", "", "", 0.0, TaxFormAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormAgreementStatus next)
    {
        switch (Status)
        {
            case TaxFormAgreementStatus.Draft:
                switch (next)
                {
                    case TaxFormAgreementStatus.Pending:
                        return true;
                }
                return false;
            case TaxFormAgreementStatus.Pending:
                switch (next)
                {
                    case TaxFormAgreementStatus.Active:
                    case TaxFormAgreementStatus.Suspended:
                        return true;
                }
                return false;
            case TaxFormAgreementStatus.Active:
                return next == TaxFormAgreementStatus.Suspended;
            case TaxFormAgreementStatus.Suspended:
                return next == TaxFormAgreementStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TaxFormAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public TaxFormAgreement Copy()
    {
        return new TaxFormAgreement(Code, Title, Source, Description, Locale, Checksum, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
