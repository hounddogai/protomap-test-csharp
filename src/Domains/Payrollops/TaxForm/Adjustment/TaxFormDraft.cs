namespace Acme.Workforce.Payrollops.TaxForm.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form draft is in its life cycle.</summary>
public enum TaxFormDraftStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tax form draft and its current status.</summary>
public sealed class TaxFormDraft(string code, string name, string location, int tenureMonths, string grade, int batchSize, int retries, string country, string currency, int quantity, int expiresAt, TaxFormDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public int BatchSize { get; set; } = batchSize;
    public int Retries { get; set; } = retries;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public int Quantity { get; set; } = quantity;
    public int ExpiresAt { get; set; } = expiresAt;
    public TaxFormDraftStatus Status { get; set; } = status;

    /// <summary>Starts a tax form draft in status draft.</summary>
    public static TaxFormDraft NewTaxFormDraft(string code, string name)
    {
        return new TaxFormDraft(code, name, "", 0, "", 0, 0, "", "", 0, 0, TaxFormDraftStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormDraftStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormDraftStatus next)
    {
        switch (Status)
        {
            case TaxFormDraftStatus.Draft:
                switch (next)
                {
                    case TaxFormDraftStatus.Closed:
                        return true;
                }
                return false;
            case TaxFormDraftStatus.Pending:
                return next == TaxFormDraftStatus.Suspended || next == TaxFormDraftStatus.Closed;
            case TaxFormDraftStatus.Active:
                return next == TaxFormDraftStatus.Suspended;
            case TaxFormDraftStatus.Suspended:
                return next == TaxFormDraftStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TaxFormDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetValue()
    {
        return Math.Round((double)BatchSize * (double)TenureMonths, 2);
    }

    public TaxFormDraft Copy()
    {
        return new TaxFormDraft(
            Code,
            Name,
            Location,
            TenureMonths,
            Grade,
            BatchSize,
            Retries,
            Country,
            Currency,
            Quantity,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
