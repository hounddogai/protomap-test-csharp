namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered retro pay forecast is in its life cycle.</summary>
public enum TieredRetroPayForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One tiered retro pay forecast, with what the planners need to act on it.</summary>
public sealed class TieredRetroPayForecast(string code, string name, bool active, int graceDays, int createdAt, int leaveDays, string phone, double rate, string employeeId, TieredRetroPayForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public TieredRetroPayForecastStatus Status { get; set; } = status;

    /// <summary>Starts a tiered retro pay forecast in status created.</summary>
    public static TieredRetroPayForecast NewTieredRetroPayForecast(string code, string name)
    {
        return new TieredRetroPayForecast(
            code,
            name,
            false,
            0,
            0,
            0,
            "",
            0.0,
            "",
            TieredRetroPayForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredRetroPayForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredRetroPayForecastStatus next)
    {
        switch (Status)
        {
            case TieredRetroPayForecastStatus.Created:
                return next == TieredRetroPayForecastStatus.Validated || next == TieredRetroPayForecastStatus.Retired;
            case TieredRetroPayForecastStatus.Validated:
                switch (next)
                {
                    case TieredRetroPayForecastStatus.Published:
                    case TieredRetroPayForecastStatus.Retired:
                        return true;
                }
                return false;
            case TieredRetroPayForecastStatus.Published:
                return next == TieredRetroPayForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TieredRetroPayForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered retro pay forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)CreatedAt - (double)LeaveDays, 2);
    }

    public TieredRetroPayForecast Copy()
    {
        return new TieredRetroPayForecast(
            Code,
            Name,
            Active,
            GraceDays,
            CreatedAt,
            LeaveDays,
            Phone,
            Rate,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
