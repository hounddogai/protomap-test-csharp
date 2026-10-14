namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment forecast is in its life cycle.</summary>
public enum GarnishmentForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A garnishment forecast as the garnishment team keeps it.</summary>
public sealed class GarnishmentForecast(string code, string name, string locale, string contactEmail, int startsAt, bool approved, GarnishmentForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public bool Approved { get; set; } = approved;
    public GarnishmentForecastStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment forecast in status new.</summary>
    public static GarnishmentForecast NewGarnishmentForecast(string code, string name)
    {
        return new GarnishmentForecast(code, name, "", "", 0, false, GarnishmentForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentForecastStatus next)
    {
        switch (Status)
        {
            case GarnishmentForecastStatus.New:
                switch (next)
                {
                    case GarnishmentForecastStatus.Queued:
                    case GarnishmentForecastStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentForecastStatus.Queued:
                return next == GarnishmentForecastStatus.Failed;
            case GarnishmentForecastStatus.Running:
                switch (next)
                {
                    case GarnishmentForecastStatus.Done:
                    case GarnishmentForecastStatus.Failed:
                        return true;
                }
                return false;
            case GarnishmentForecastStatus.Done:
                return next == GarnishmentForecastStatus.Failed;
        }
        return false;
    }

    public void MoveTo(GarnishmentForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public GarnishmentForecast Copy()
    {
        return new GarnishmentForecast(Code, Name, Locale, ContactEmail, StartsAt, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
