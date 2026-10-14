namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat forecast is in its life cycle.</summary>
public enum LicenseSeatForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One license seat forecast, with what the planners need to act on it.</summary>
public sealed class LicenseSeatForecast(string code, string name, double ceiling, int endsAt, int batchSize, int expiresAt, double hours, double budget, string description, string checksum, string title, LicenseSeatForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public double Budget { get; set; } = budget;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public string Title { get; set; } = title;
    public LicenseSeatForecastStatus Status { get; set; } = status;

    /// <summary>Starts a license seat forecast in status idle.</summary>
    public static LicenseSeatForecast NewLicenseSeatForecast(string code, string name)
    {
        return new LicenseSeatForecast(
            code,
            name,
            0.0,
            0,
            0,
            0,
            0.0,
            0.0,
            "",
            "",
            "",
            LicenseSeatForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatForecastStatus next)
    {
        switch (Status)
        {
            case LicenseSeatForecastStatus.Idle:
                return next == LicenseSeatForecastStatus.Stopped;
            case LicenseSeatForecastStatus.Warming:
                switch (next)
                {
                    case LicenseSeatForecastStatus.Draining:
                        return true;
                }
                return false;
            case LicenseSeatForecastStatus.Ready:
                switch (next)
                {
                    case LicenseSeatForecastStatus.Draining:
                        return true;
                }
                return false;
            case LicenseSeatForecastStatus.Draining:
                return next == LicenseSeatForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LicenseSeatForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"license seat forecast"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Budget + (double)ExpiresAt, 2);
    }

    public LicenseSeatForecast Copy()
    {
        return new LicenseSeatForecast(
            Code,
            Name,
            Ceiling,
            EndsAt,
            BatchSize,
            ExpiresAt,
            Hours,
            Budget,
            Description,
            Checksum,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
