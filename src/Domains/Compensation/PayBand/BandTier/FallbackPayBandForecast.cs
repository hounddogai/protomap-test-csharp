namespace Acme.Workforce.Compensation.PayBand.BandTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay band forecast is in its life cycle.</summary>
public enum FallbackPayBandForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback pay band forecast as the pay band team keeps it.</summary>
public sealed class FallbackPayBandForecast(string code, string label, string source, double discount, double maximum, FallbackPayBandForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public double Discount { get; set; } = discount;
    public double Maximum { get; set; } = maximum;
    public FallbackPayBandForecastStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay band forecast in status proposed.</summary>
    public static FallbackPayBandForecast NewFallbackPayBandForecast(string code, string label)
    {
        return new FallbackPayBandForecast(code, label, "", 0.0, 0.0, FallbackPayBandForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayBandForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayBandForecastStatus next)
    {
        switch (Status)
        {
            case FallbackPayBandForecastStatus.Proposed:
                switch (next)
                {
                    case FallbackPayBandForecastStatus.Rejected:
                    case FallbackPayBandForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackPayBandForecastStatus.Approved:
                return next == FallbackPayBandForecastStatus.Rejected || next == FallbackPayBandForecastStatus.Withdrawn;
            case FallbackPayBandForecastStatus.Rejected:
                switch (next)
                {
                    case FallbackPayBandForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPayBandForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay band forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Maximum - Discount, 2);
    }

    public FallbackPayBandForecast Copy()
    {
        return new FallbackPayBandForecast(Code, Label, Source, Discount, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
