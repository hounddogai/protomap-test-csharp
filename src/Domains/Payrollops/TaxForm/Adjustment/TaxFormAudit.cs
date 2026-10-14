namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form audit is in its life cycle.</summary>
public enum TaxFormAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tax form audit. Its code is unique within a tenant.</summary>
public sealed class TaxFormAudit(string code, string title, int revision, string location, double margin, double rate, string grade, string ownerId, string contactEmail, string locale, double ceiling, TaxFormAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public string ContactEmail { get; set; } = contactEmail;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public TaxFormAuditStatus Status { get; set; } = status;

    /// <summary>Starts a tax form audit in status created.</summary>
    public static TaxFormAudit NewTaxFormAudit(string code, string title)
    {
        return new TaxFormAudit(code, title, 0, "", 0.0, 0.0, "", "", "", "", 0.0, TaxFormAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormAuditStatus next)
    {
        switch (Status)
        {
            case TaxFormAuditStatus.Created:
                switch (next)
                {
                    case TaxFormAuditStatus.Validated:
                        return true;
                }
                return false;
            case TaxFormAuditStatus.Validated:
                return next == TaxFormAuditStatus.Published || next == TaxFormAuditStatus.Retired;
            case TaxFormAuditStatus.Published:
                switch (next)
                {
                    case TaxFormAuditStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rate - (double)Revision, 2);
    }

    public TaxFormAudit Copy()
    {
        return new TaxFormAudit(
            Code,
            Title,
            Revision,
            Location,
            Margin,
            Rate,
            Grade,
            OwnerId,
            ContactEmail,
            Locale,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
