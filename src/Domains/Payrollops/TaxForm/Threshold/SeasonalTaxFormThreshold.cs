namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal tax form threshold is in its life cycle.</summary>
public enum SeasonalTaxFormThresholdStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal tax form threshold, with what the planners need to act on it.</summary>
public sealed class SeasonalTaxFormThreshold(string code, string label, double margin, string department, string jobCode, double threshold, bool archived, string currency, SeasonalTaxFormThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public string JobCode { get; set; } = jobCode;
    public double Threshold { get; set; } = threshold;
    public bool Archived { get; set; } = archived;
    public string Currency { get; set; } = currency;
    public SeasonalTaxFormThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal tax form threshold in status created.</summary>
    public static SeasonalTaxFormThreshold NewSeasonalTaxFormThreshold(string code, string label)
    {
        return new SeasonalTaxFormThreshold(
            code,
            label,
            0.0,
            "",
            "",
            0.0,
            false,
            "",
            SeasonalTaxFormThresholdStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTaxFormThresholdStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTaxFormThresholdStatus next)
    {
        switch (Status)
        {
            case SeasonalTaxFormThresholdStatus.Created:
                return next == SeasonalTaxFormThresholdStatus.Retired;
            case SeasonalTaxFormThresholdStatus.Validated:
                return next == SeasonalTaxFormThresholdStatus.Retired;
            case SeasonalTaxFormThresholdStatus.Published:
                return next == SeasonalTaxFormThresholdStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SeasonalTaxFormThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal tax form threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Threshold - Margin, 2);
    }

    public SeasonalTaxFormThreshold Copy()
    {
        return new SeasonalTaxFormThreshold(
            Code,
            Label,
            Margin,
            Department,
            JobCode,
            Threshold,
            Archived,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
