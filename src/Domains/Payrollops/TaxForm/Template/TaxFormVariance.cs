namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form variance is in its life cycle.</summary>
public enum TaxFormVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form variance as the tax form team keeps it.</summary>
public sealed class TaxFormVariance(string code, string label, int attempts, string grade, int quantity, string managerId, string locale, string segment, double margin, TaxFormVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Attempts { get; set; } = attempts;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public TaxFormVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tax form variance in status planned.</summary>
    public static TaxFormVariance NewTaxFormVariance(string code, string label)
    {
        return new TaxFormVariance(code, label, 0, "", 0, "", "", "", 0.0, TaxFormVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormVarianceStatus next)
    {
        switch (Status)
        {
            case TaxFormVarianceStatus.Planned:
                switch (next)
                {
                    case TaxFormVarianceStatus.Scheduled:
                    case TaxFormVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case TaxFormVarianceStatus.Scheduled:
                switch (next)
                {
                    case TaxFormVarianceStatus.Completed:
                    case TaxFormVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case TaxFormVarianceStatus.InProgress:
                return next == TaxFormVarianceStatus.Completed;
            case TaxFormVarianceStatus.Completed:
                return next == TaxFormVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TaxFormVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin * (double)Attempts, 2);
    }

    public TaxFormVariance Copy()
    {
        return new TaxFormVariance(
            Code,
            Label,
            Attempts,
            Grade,
            Quantity,
            ManagerId,
            Locale,
            Segment,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
