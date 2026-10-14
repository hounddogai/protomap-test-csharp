namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default license seat forecast is in its life cycle.</summary>
public enum DefaultLicenseSeatForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A default license seat forecast. Its code is unique within a tenant.</summary>
public sealed class DefaultLicenseSeatForecast(string code, string label, double fte, string timezone, int batchSize, double minimum, double hours, double weight, string skill, DefaultLicenseSeatForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public int BatchSize { get; set; } = batchSize;
    public double Minimum { get; set; } = minimum;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public string Skill { get; set; } = skill;
    public DefaultLicenseSeatForecastStatus Status { get; set; } = status;

    /// <summary>Starts a default license seat forecast in status created.</summary>
    public static DefaultLicenseSeatForecast NewDefaultLicenseSeatForecast(string code, string label)
    {
        return new DefaultLicenseSeatForecast(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            0.0,
            0.0,
            "",
            DefaultLicenseSeatForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultLicenseSeatForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultLicenseSeatForecastStatus next)
    {
        switch (Status)
        {
            case DefaultLicenseSeatForecastStatus.Created:
                return next == DefaultLicenseSeatForecastStatus.Validated || next == DefaultLicenseSeatForecastStatus.Retired;
            case DefaultLicenseSeatForecastStatus.Validated:
                return next == DefaultLicenseSeatForecastStatus.Retired;
            case DefaultLicenseSeatForecastStatus.Published:
                switch (next)
                {
                    case DefaultLicenseSeatForecastStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultLicenseSeatForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default license seat forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default license seat forecast"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round(Minimum * (double)BatchSize, 2);
    }

    public DefaultLicenseSeatForecast Copy()
    {
        return new DefaultLicenseSeatForecast(
            Code,
            Label,
            Fte,
            Timezone,
            BatchSize,
            Minimum,
            Hours,
            Weight,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
