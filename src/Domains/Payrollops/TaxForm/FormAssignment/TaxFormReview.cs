namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form review is in its life cycle.</summary>
public enum TaxFormReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a tax form review and its current status.</summary>
public sealed class TaxFormReview(string code, string label, double minimum, double weight, double floorValue, int durationMinutes, int expiresAt, string source, string unit, string locale, TaxFormReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Weight { get; set; } = weight;
    public double FloorValue { get; set; } = floorValue;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Source { get; set; } = source;
    public string Unit { get; set; } = unit;
    public string Locale { get; set; } = locale;
    public TaxFormReviewStatus Status { get; set; } = status;

    /// <summary>Starts a tax form review in status draft.</summary>
    public static TaxFormReview NewTaxFormReview(string code, string label)
    {
        return new TaxFormReview(code, label, 0.0, 0.0, 0.0, 0, 0, "", "", "", TaxFormReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormReviewStatus next)
    {
        switch (Status)
        {
            case TaxFormReviewStatus.Draft:
                return next == TaxFormReviewStatus.Pending || next == TaxFormReviewStatus.Suspended;
            case TaxFormReviewStatus.Pending:
                return next == TaxFormReviewStatus.Suspended || next == TaxFormReviewStatus.Closed;
            case TaxFormReviewStatus.Active:
                switch (next)
                {
                    case TaxFormReviewStatus.Suspended:
                    case TaxFormReviewStatus.Closed:
                        return true;
                }
                return false;
            case TaxFormReviewStatus.Suspended:
                switch (next)
                {
                    case TaxFormReviewStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round((double)DurationMinutes - (double)ExpiresAt, 2);
    }

    public TaxFormReview Copy()
    {
        return new TaxFormReview(
            Code,
            Label,
            Minimum,
            Weight,
            FloorValue,
            DurationMinutes,
            ExpiresAt,
            Source,
            Unit,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
