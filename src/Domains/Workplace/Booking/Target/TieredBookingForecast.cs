namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered booking forecast is in its life cycle.</summary>
public enum TieredBookingForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a tiered booking forecast and its current status.</summary>
public sealed class TieredBookingForecast(string code, string label, string name, int tenureMonths, double hours, bool pinned, bool active, double rate, int durationMinutes, int version, double ceiling, TieredBookingForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Hours { get; set; } = hours;
    public bool Pinned { get; set; } = pinned;
    public bool Active { get; set; } = active;
    public double Rate { get; set; } = rate;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Version { get; set; } = version;
    public double Ceiling { get; set; } = ceiling;
    public TieredBookingForecastStatus Status { get; set; } = status;

    /// <summary>Starts a tiered booking forecast in status requested.</summary>
    public static TieredBookingForecast NewTieredBookingForecast(string code, string label)
    {
        return new TieredBookingForecast(
            code,
            label,
            "",
            0,
            0.0,
            false,
            false,
            0.0,
            0,
            0,
            0.0,
            TieredBookingForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredBookingForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredBookingForecastStatus next)
    {
        switch (Status)
        {
            case TieredBookingForecastStatus.Requested:
                switch (next)
                {
                    case TieredBookingForecastStatus.Expired:
                        return true;
                }
                return false;
            case TieredBookingForecastStatus.Granted:
                switch (next)
                {
                    case TieredBookingForecastStatus.Expired:
                        return true;
                }
                return false;
            case TieredBookingForecastStatus.Revoked:
                return next == TieredBookingForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TieredBookingForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered booking forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"tiered booking forecast"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Ceiling - (double)DurationMinutes, 2);
    }

    public TieredBookingForecast Copy()
    {
        return new TieredBookingForecast(
            Code,
            Label,
            Name,
            TenureMonths,
            Hours,
            Pinned,
            Active,
            Rate,
            DurationMinutes,
            Version,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
