namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form baseline is in its life cycle.</summary>
public enum TaxFormBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tax form baseline as the tax form team keeps it.</summary>
public sealed class TaxFormBaseline(string code, string title, int priority, int quantity, string contactEmail, string notes, string country, TaxFormBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public int Quantity { get; set; } = quantity;
    public string ContactEmail { get; set; } = contactEmail;
    public string Notes { get; set; } = notes;
    public string Country { get; set; } = country;
    public TaxFormBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a tax form baseline in status open.</summary>
    public static TaxFormBaseline NewTaxFormBaseline(string code, string title)
    {
        return new TaxFormBaseline(code, title, 0, 0, "", "", "", TaxFormBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormBaselineStatus next)
    {
        switch (Status)
        {
            case TaxFormBaselineStatus.Open:
                switch (next)
                {
                    case TaxFormBaselineStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormBaselineStatus.InReview:
                switch (next)
                {
                    case TaxFormBaselineStatus.Reopened:
                    case TaxFormBaselineStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormBaselineStatus.Resolved:
                switch (next)
                {
                    case TaxFormBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case TaxFormBaselineStatus.Reopened:
                return next == TaxFormBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(TaxFormBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetTotal()
    {
        return Math.Round((double)Quantity - (double)Priority, 2);
    }

    public TaxFormBaseline Copy()
    {
        return new TaxFormBaseline(Code, Title, Priority, Quantity, ContactEmail, Notes, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
