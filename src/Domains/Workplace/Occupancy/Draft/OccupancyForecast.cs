namespace Acme.Workforce.Workplace.Occupancy.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy forecast is in its life cycle.</summary>
public enum OccupancyForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One occupancy forecast, with what the planners need to act on it.</summary>
public sealed class OccupancyForecast(string code, string title, double margin, int expiresAt, string team, string country, string contactEmail, string region, OccupancyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Margin { get; set; } = margin;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Team { get; set; } = team;
    public string Country { get; set; } = country;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public OccupancyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy forecast in status proposed.</summary>
    public static OccupancyForecast NewOccupancyForecast(string code, string title)
    {
        return new OccupancyForecast(code, title, 0.0, 0, "", "", "", "", OccupancyForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyForecastStatus next)
    {
        switch (Status)
        {
            case OccupancyForecastStatus.Proposed:
                switch (next)
                {
                    case OccupancyForecastStatus.Rejected:
                        return true;
                }
                return false;
            case OccupancyForecastStatus.Approved:
                switch (next)
                {
                    case OccupancyForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case OccupancyForecastStatus.Rejected:
                switch (next)
                {
                    case OccupancyForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(OccupancyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round((double)ExpiresAt + Margin, 2);
    }

    public OccupancyForecast Copy()
    {
        return new OccupancyForecast(Code, Title, Margin, ExpiresAt, Team, Country, ContactEmail, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
