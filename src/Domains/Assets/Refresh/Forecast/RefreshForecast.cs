namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh forecast is in its life cycle.</summary>
public enum RefreshForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One refresh forecast, with what the planners need to act on it.</summary>
public sealed class RefreshForecast(string code, string title, string channel, double ratio, int retries, int sequence, int version, string summary, RefreshForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public double Ratio { get; set; } = ratio;
    public int Retries { get; set; } = retries;
    public int Sequence { get; set; } = sequence;
    public int Version { get; set; } = version;
    public string Summary { get; set; } = summary;
    public RefreshForecastStatus Status { get; set; } = status;

    /// <summary>Starts a refresh forecast in status requested.</summary>
    public static RefreshForecast NewRefreshForecast(string code, string title)
    {
        return new RefreshForecast(code, title, "", 0.0, 0, 0, 0, "", RefreshForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshForecastStatus next)
    {
        switch (Status)
        {
            case RefreshForecastStatus.Requested:
                return next == RefreshForecastStatus.Granted || next == RefreshForecastStatus.Expired;
            case RefreshForecastStatus.Granted:
                return next == RefreshForecastStatus.Expired;
            case RefreshForecastStatus.Revoked:
                return next == RefreshForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RefreshForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Retries + (double)Sequence, 2);
    }

    public RefreshForecast Copy()
    {
        return new RefreshForecast(Code, Title, Channel, Ratio, Retries, Sequence, Version, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
