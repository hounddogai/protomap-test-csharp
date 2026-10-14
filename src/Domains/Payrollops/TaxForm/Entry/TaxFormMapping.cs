namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form mapping is in its life cycle.</summary>
public enum TaxFormMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A tax form mapping. Its code is unique within a tenant.</summary>
public sealed class TaxFormMapping(string code, string label, double rating, string region, int limit, double discount, int quantity, int tenureMonths, TaxFormMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public string Region { get; set; } = region;
    public int Limit { get; set; } = limit;
    public double Discount { get; set; } = discount;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public TaxFormMappingStatus Status { get; set; } = status;

    /// <summary>Starts a tax form mapping in status open.</summary>
    public static TaxFormMapping NewTaxFormMapping(string code, string label)
    {
        return new TaxFormMapping(code, label, 0.0, "", 0, 0.0, 0, 0, TaxFormMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormMappingStatus next)
    {
        switch (Status)
        {
            case TaxFormMappingStatus.Open:
                return next == TaxFormMappingStatus.InReview;
            case TaxFormMappingStatus.InReview:
                return next == TaxFormMappingStatus.Archived;
            case TaxFormMappingStatus.Resolved:
                return next == TaxFormMappingStatus.Reopened || next == TaxFormMappingStatus.Archived;
            case TaxFormMappingStatus.Reopened:
                switch (next)
                {
                    case TaxFormMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Discount + (double)Limit, 2);
    }

    public TaxFormMapping Copy()
    {
        return new TaxFormMapping(Code, Label, Rating, Region, Limit, Discount, Quantity, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
