namespace Acme.Workforce.Payrollops.TaxForm.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form override is in its life cycle.</summary>
public enum TaxFormOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A tax form override as the tax form team keeps it.</summary>
public sealed class TaxFormOverride(string code, string title, int version, double minimum, int limit, int expiresAt, double rate, string salaryBand, double budget, bool approved, string source, TaxFormOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Rate { get; set; } = rate;
    public string SalaryBand { get; set; } = salaryBand;
    public double Budget { get; set; } = budget;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public TaxFormOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a tax form override in status planned.</summary>
    public static TaxFormOverride NewTaxFormOverride(string code, string title)
    {
        return new TaxFormOverride(
            code,
            title,
            0,
            0.0,
            0,
            0,
            0.0,
            "",
            0.0,
            false,
            "",
            TaxFormOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormOverrideStatus next)
    {
        switch (Status)
        {
            case TaxFormOverrideStatus.Planned:
                return next == TaxFormOverrideStatus.Cancelled;
            case TaxFormOverrideStatus.Scheduled:
                switch (next)
                {
                    case TaxFormOverrideStatus.Completed:
                        return true;
                }
                return false;
            case TaxFormOverrideStatus.InProgress:
                switch (next)
                {
                    case TaxFormOverrideStatus.Completed:
                    case TaxFormOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case TaxFormOverrideStatus.Completed:
                return next == TaxFormOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(TaxFormOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"tax form override"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum * (double)ExpiresAt, 2);
    }

    public TaxFormOverride Copy()
    {
        return new TaxFormOverride(
            Code,
            Title,
            Version,
            Minimum,
            Limit,
            ExpiresAt,
            Rate,
            SalaryBand,
            Budget,
            Approved,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
