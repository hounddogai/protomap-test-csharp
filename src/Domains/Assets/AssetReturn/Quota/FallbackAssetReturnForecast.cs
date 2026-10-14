namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback asset return forecast is in its life cycle.</summary>
public enum FallbackAssetReturnForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One fallback asset return forecast, with what the planners need to act on it.</summary>
public sealed class FallbackAssetReturnForecast(string code, string name, bool approved, int durationMinutes, double ratio, int graceDays, double hours, double weight, FallbackAssetReturnForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ratio { get; set; } = ratio;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public FallbackAssetReturnForecastStatus Status { get; set; } = status;

    /// <summary>Starts a fallback asset return forecast in status requested.</summary>
    public static FallbackAssetReturnForecast NewFallbackAssetReturnForecast(string code, string name)
    {
        return new FallbackAssetReturnForecast(
            code,
            name,
            false,
            0,
            0.0,
            0,
            0.0,
            0.0,
            FallbackAssetReturnForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAssetReturnForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAssetReturnForecastStatus next)
    {
        switch (Status)
        {
            case FallbackAssetReturnForecastStatus.Requested:
                switch (next)
                {
                    case FallbackAssetReturnForecastStatus.Granted:
                    case FallbackAssetReturnForecastStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackAssetReturnForecastStatus.Granted:
                return next == FallbackAssetReturnForecastStatus.Revoked;
            case FallbackAssetReturnForecastStatus.Revoked:
                return next == FallbackAssetReturnForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(FallbackAssetReturnForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback asset return forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback asset return forecast"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes - (double)GraceDays, 2);
    }

    public FallbackAssetReturnForecast Copy()
    {
        return new FallbackAssetReturnForecast(
            Code,
            Name,
            Approved,
            DurationMinutes,
            Ratio,
            GraceDays,
            Hours,
            Weight,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
