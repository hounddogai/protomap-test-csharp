namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default pool forecast is in its life cycle.</summary>
public enum DefaultPoolForecastStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a default pool forecast and its current status.</summary>
public sealed class DefaultPoolForecast(string code, string label, string location, double threshold, int retries, double cost, double amount, double weight, double fte, DefaultPoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public double Amount { get; set; } = amount;
    public double Weight { get; set; } = weight;
    public double Fte { get; set; } = fte;
    public DefaultPoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a default pool forecast in status requested.</summary>
    public static DefaultPoolForecast NewDefaultPoolForecast(string code, string label)
    {
        return new DefaultPoolForecast(
            code,
            label,
            "",
            0.0,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            DefaultPoolForecastStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPoolForecastStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPoolForecastStatus next)
    {
        switch (Status)
        {
            case DefaultPoolForecastStatus.Requested:
                switch (next)
                {
                    case DefaultPoolForecastStatus.Granted:
                    case DefaultPoolForecastStatus.Expired:
                        return true;
                }
                return false;
            case DefaultPoolForecastStatus.Granted:
                switch (next)
                {
                    case DefaultPoolForecastStatus.Revoked:
                    case DefaultPoolForecastStatus.Expired:
                        return true;
                }
                return false;
            case DefaultPoolForecastStatus.Revoked:
                switch (next)
                {
                    case DefaultPoolForecastStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default pool forecast"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Weight - (double)Retries, 2);
    }

    public DefaultPoolForecast Copy()
    {
        return new DefaultPoolForecast(Code, Label, Location, Threshold, Retries, Cost, Amount, Weight, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
