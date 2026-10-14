namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant forecast is in its life cycle.</summary>
public enum EquityGrantForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One equity grant forecast, with what the planners need to act on it.</summary>
public sealed class EquityGrantForecast(string code, string title, string costCenter, string managerId, double rate, string timezone, int limit, string unit, string reviewer, int expiresAt, string location, EquityGrantForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public string Reviewer { get; set; } = reviewer;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Location { get; set; } = location;
    public EquityGrantForecastStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant forecast in status draft.</summary>
    public static EquityGrantForecast NewEquityGrantForecast(string code, string title)
    {
        return new EquityGrantForecast(
            code,
            title,
            "",
            "",
            0.0,
            "",
            0,
            "",
            "",
            0,
            "",
            EquityGrantForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantForecastStatus next)
    {
        switch (Status)
        {
            case EquityGrantForecastStatus.Draft:
                return next == EquityGrantForecastStatus.Active;
            case EquityGrantForecastStatus.Pending:
                return next == EquityGrantForecastStatus.Closed;
            case EquityGrantForecastStatus.Active:
                switch (next)
                {
                    case EquityGrantForecastStatus.Suspended:
                    case EquityGrantForecastStatus.Closed:
                        return true;
                }
                return false;
            case EquityGrantForecastStatus.Suspended:
                return next == EquityGrantForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(EquityGrantForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"equity grant forecast"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)ExpiresAt - Rate, 2);
    }

    public EquityGrantForecast Copy()
    {
        return new EquityGrantForecast(
            Code,
            Title,
            CostCenter,
            ManagerId,
            Rate,
            Timezone,
            Limit,
            Unit,
            Reviewer,
            ExpiresAt,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
