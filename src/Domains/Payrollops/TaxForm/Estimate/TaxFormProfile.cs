namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form profile is in its life cycle.</summary>
public enum TaxFormProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form profile. Its code is unique within a tenant.</summary>
public sealed class TaxFormProfile(string code, string name, double amount, string locale, string category, string title, double weight, int durationMinutes, double rate, int graceDays, TaxFormProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Locale { get; set; } = locale;
    public string Category { get; set; } = category;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public int GraceDays { get; set; } = graceDays;
    public TaxFormProfileStatus Status { get; set; } = status;

    /// <summary>Starts a tax form profile in status planned.</summary>
    public static TaxFormProfile NewTaxFormProfile(string code, string name)
    {
        return new TaxFormProfile(code, name, 0.0, "", "", "", 0.0, 0, 0.0, 0, TaxFormProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormProfileStatus next)
    {
        switch (Status)
        {
            case TaxFormProfileStatus.Planned:
                return next == TaxFormProfileStatus.InProgress || next == TaxFormProfileStatus.Completed;
            case TaxFormProfileStatus.Scheduled:
                switch (next)
                {
                    case TaxFormProfileStatus.InProgress:
                        return true;
                }
                return false;
            case TaxFormProfileStatus.InProgress:
                return next == TaxFormProfileStatus.Completed || next == TaxFormProfileStatus.Cancelled;
            case TaxFormProfileStatus.Completed:
                switch (next)
                {
                    case TaxFormProfileStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TaxFormProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)GraceDays - Amount, 2);
    }

    public TaxFormProfile Copy()
    {
        return new TaxFormProfile(
            Code,
            Name,
            Amount,
            Locale,
            Category,
            Title,
            Weight,
            DurationMinutes,
            Rate,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
