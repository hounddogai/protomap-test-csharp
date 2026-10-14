namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band forecast is in its life cycle.</summary>
public enum PayBandForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A pay band forecast. Its code is unique within a tenant.</summary>
public sealed class PayBandForecast(string code, string label, string contactEmail, string department, double rating, double rate, PayBandForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string Department { get; set; } = department;
    public double Rating { get; set; } = rating;
    public double Rate { get; set; } = rate;
    public PayBandForecastStatus Status { get; set; } = status;

    /// <summary>Starts a pay band forecast in status created.</summary>
    public static PayBandForecast NewPayBandForecast(string code, string label)
    {
        return new PayBandForecast(code, label, "", "", 0.0, 0.0, PayBandForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandForecastStatus next)
    {
        switch (Status)
        {
            case PayBandForecastStatus.Created:
                switch (next)
                {
                    case PayBandForecastStatus.Published:
                        return true;
                }
                return false;
            case PayBandForecastStatus.Validated:
                switch (next)
                {
                    case PayBandForecastStatus.Published:
                        return true;
                }
                return false;
            case PayBandForecastStatus.Published:
                switch (next)
                {
                    case PayBandForecastStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PayBandForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay band forecast"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Rating * Rate, 2);
    }

    public PayBandForecast Copy()
    {
        return new PayBandForecast(Code, Label, ContactEmail, Department, Rating, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
