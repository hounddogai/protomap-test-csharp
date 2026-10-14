namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form forecast is in its life cycle.</summary>
public enum TaxFormForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One tax form forecast, with what the planners need to act on it.</summary>
public sealed class TaxFormForecast(string code, string title, double hours, int createdAt, int version, int endsAt, TaxFormForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int CreatedAt { get; set; } = createdAt;
    public int Version { get; set; } = version;
    public int EndsAt { get; set; } = endsAt;
    public TaxFormForecastStatus Status { get; set; } = status;

    /// <summary>Starts a tax form forecast in status requested.</summary>
    public static TaxFormForecast NewTaxFormForecast(string code, string title)
    {
        return new TaxFormForecast(code, title, 0.0, 0, 0, 0, TaxFormForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormForecastStatus next)
    {
        switch (Status)
        {
            case TaxFormForecastStatus.Requested:
                switch (next)
                {
                    case TaxFormForecastStatus.Granted:
                        return true;
                }
                return false;
            case TaxFormForecastStatus.Granted:
                return next == TaxFormForecastStatus.Revoked;
            case TaxFormForecastStatus.Revoked:
                return next == TaxFormForecastStatus.Expired;
        }
        return false;
    }

    public void MoveTo(TaxFormForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt * (double)EndsAt, 2);
    }

    public TaxFormForecast Copy()
    {
        return new TaxFormForecast(Code, Title, Hours, CreatedAt, Version, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
