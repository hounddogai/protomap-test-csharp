namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form exception is in its life cycle.</summary>
public enum TaxFormExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tax form exception, with what the planners need to act on it.</summary>
public sealed class TaxFormException(string code, string title, string category, string currency, string costCenter, int quantity, TaxFormExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public string CostCenter { get; set; } = costCenter;
    public int Quantity { get; set; } = quantity;
    public TaxFormExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a tax form exception in status open.</summary>
    public static TaxFormException NewTaxFormException(string code, string title)
    {
        return new TaxFormException(code, title, "", "", "", 0, TaxFormExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormExceptionStatus next)
    {
        switch (Status)
        {
            case TaxFormExceptionStatus.Open:
                return next == TaxFormExceptionStatus.Reopened;
            case TaxFormExceptionStatus.InReview:
                switch (next)
                {
                    case TaxFormExceptionStatus.Archived:
                        return true;
                }
                return false;
            case TaxFormExceptionStatus.Resolved:
                return next == TaxFormExceptionStatus.Reopened || next == TaxFormExceptionStatus.Archived;
            case TaxFormExceptionStatus.Reopened:
                switch (next)
                {
                    case TaxFormExceptionStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"tax form exception"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public TaxFormException Copy()
    {
        return new TaxFormException(Code, Title, Category, Currency, CostCenter, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
