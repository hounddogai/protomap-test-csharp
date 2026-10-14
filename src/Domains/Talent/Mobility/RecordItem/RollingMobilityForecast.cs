namespace Acme.Workforce.Talent.Mobility.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling mobility forecast is in its life cycle.</summary>
public enum RollingMobilityForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling mobility forecast as the mobility team keeps it.</summary>
public sealed class RollingMobilityForecast(string code, string title, string contactEmail, string description, int quantity, string location, RollingMobilityForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public string Location { get; set; } = location;
    public RollingMobilityForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rolling mobility forecast in status created.</summary>
    public static RollingMobilityForecast NewRollingMobilityForecast(string code, string title)
    {
        return new RollingMobilityForecast(code, title, "", "", 0, "", RollingMobilityForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingMobilityForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingMobilityForecastStatus next)
    {
        switch (Status)
        {
            case RollingMobilityForecastStatus.Created:
                switch (next)
                {
                    case RollingMobilityForecastStatus.Validated:
                    case RollingMobilityForecastStatus.Published:
                        return true;
                }
                return false;
            case RollingMobilityForecastStatus.Validated:
                switch (next)
                {
                    case RollingMobilityForecastStatus.Published:
                    case RollingMobilityForecastStatus.Retired:
                        return true;
                }
                return false;
            case RollingMobilityForecastStatus.Published:
                return next == RollingMobilityForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingMobilityForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling mobility forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling mobility forecast"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public RollingMobilityForecast Copy()
    {
        return new RollingMobilityForecast(Code, Title, ContactEmail, Description, Quantity, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
