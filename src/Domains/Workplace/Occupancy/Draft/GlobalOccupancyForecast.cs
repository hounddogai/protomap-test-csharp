namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global occupancy forecast is in its life cycle.</summary>
public enum GlobalOccupancyForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One global occupancy forecast, with what the planners need to act on it.</summary>
public sealed class GlobalOccupancyForecast(string code, string label, bool active, double ceiling, string externalRef, string source, string region, int updatedAt, GlobalOccupancyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public double Ceiling { get; set; } = ceiling;
    public string ExternalRef { get; set; } = externalRef;
    public string Source { get; set; } = source;
    public string Region { get; set; } = region;
    public int UpdatedAt { get; set; } = updatedAt;
    public GlobalOccupancyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a global occupancy forecast in status requested.</summary>
    public static GlobalOccupancyForecast NewGlobalOccupancyForecast(string code, string label)
    {
        return new GlobalOccupancyForecast(
            code,
            label,
            false,
            0.0,
            "",
            "",
            "",
            0,
            GlobalOccupancyForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalOccupancyForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalOccupancyForecastStatus next)
    {
        switch (Status)
        {
            case GlobalOccupancyForecastStatus.Requested:
                switch (next)
                {
                    case GlobalOccupancyForecastStatus.Revoked:
                        return true;
                }
                return false;
            case GlobalOccupancyForecastStatus.Granted:
                switch (next)
                {
                    case GlobalOccupancyForecastStatus.Revoked:
                    case GlobalOccupancyForecastStatus.Expired:
                        return true;
                }
                return false;
            case GlobalOccupancyForecastStatus.Revoked:
                return next == GlobalOccupancyForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalOccupancyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global occupancy forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global occupancy forecast"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)UpdatedAt - Ceiling, 2);
    }

    public GlobalOccupancyForecast Copy()
    {
        return new GlobalOccupancyForecast(
            Code,
            Label,
            Active,
            Ceiling,
            ExternalRef,
            Source,
            Region,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
