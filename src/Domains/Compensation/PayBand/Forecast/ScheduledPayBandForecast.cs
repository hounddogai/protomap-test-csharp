namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band forecast is in its life cycle.</summary>
public enum ScheduledPayBandForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a scheduled pay band forecast and its current status.</summary>
public sealed class ScheduledPayBandForecast(string code, string label, int tenureMonths, string team, string name, ScheduledPayBandForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string Name { get; set; } = name;
    public ScheduledPayBandForecastStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band forecast in status planned.</summary>
    public static ScheduledPayBandForecast NewScheduledPayBandForecast(string code, string label)
    {
        return new ScheduledPayBandForecast(code, label, 0, "", "", ScheduledPayBandForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandForecastStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandForecastStatus.Planned:
                return next == ScheduledPayBandForecastStatus.Scheduled;
            case ScheduledPayBandForecastStatus.Scheduled:
                return next == ScheduledPayBandForecastStatus.Completed;
            case ScheduledPayBandForecastStatus.InProgress:
                return next == ScheduledPayBandForecastStatus.Completed;
            case ScheduledPayBandForecastStatus.Completed:
                switch (next)
                {
                    case ScheduledPayBandForecastStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public ScheduledPayBandForecast Copy()
    {
        return new ScheduledPayBandForecast(Code, Label, TenureMonths, Team, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
