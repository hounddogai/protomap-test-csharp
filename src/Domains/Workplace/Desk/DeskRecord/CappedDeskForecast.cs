namespace Acme.Workforce.Workplace.Desk.DeskRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped desk forecast is in its life cycle.</summary>
public enum CappedDeskForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One capped desk forecast, with what the planners need to act on it.</summary>
public sealed class CappedDeskForecast(string code, string name, string jobCode, string currency, int version, double floorValue, string contactEmail, string timezone, int graceDays, string externalRef, CappedDeskForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Currency { get; set; } = currency;
    public int Version { get; set; } = version;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public string ExternalRef { get; set; } = externalRef;
    public CappedDeskForecastStatus Status { get; set; } = status;

    /// <summary>Starts a capped desk forecast in status new.</summary>
    public static CappedDeskForecast NewCappedDeskForecast(string code, string name)
    {
        return new CappedDeskForecast(code, name, "", "", 0, 0.0, "", "", 0, "", CappedDeskForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedDeskForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedDeskForecastStatus next)
    {
        switch (Status)
        {
            case CappedDeskForecastStatus.New:
                switch (next)
                {
                    case CappedDeskForecastStatus.Failed:
                        return true;
                }
                return false;
            case CappedDeskForecastStatus.Queued:
                switch (next)
                {
                    case CappedDeskForecastStatus.Failed:
                        return true;
                }
                return false;
            case CappedDeskForecastStatus.Running:
                switch (next)
                {
                    case CappedDeskForecastStatus.Done:
                        return true;
                }
                return false;
            case CappedDeskForecastStatus.Done:
                return next == CappedDeskForecastStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedDeskForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped desk forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"capped desk forecast"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue * (double)GraceDays, 2);
    }

    public CappedDeskForecast Copy()
    {
        return new CappedDeskForecast(
            Code,
            Name,
            JobCode,
            Currency,
            Version,
            FloorValue,
            ContactEmail,
            Timezone,
            GraceDays,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
