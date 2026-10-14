namespace Acme.Workforce.Assets.Laptop.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a laptop forecast is in its life cycle.</summary>
public enum LaptopForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A laptop forecast as the laptop team keeps it.</summary>
public sealed class LaptopForecast(string code, string title, string label, double maximum, double threshold, bool pinned, string summary, double cost, LaptopForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public bool Pinned { get; set; } = pinned;
    public string Summary { get; set; } = summary;
    public double Cost { get; set; } = cost;
    public LaptopForecastStatus Status { get; set; } = status;

    /// <summary>Starts a laptop forecast in status idle.</summary>
    public static LaptopForecast NewLaptopForecast(string code, string title)
    {
        return new LaptopForecast(code, title, "", 0.0, 0.0, false, "", 0.0, LaptopForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LaptopForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LaptopForecastStatus next)
    {
        switch (Status)
        {
            case LaptopForecastStatus.Idle:
                return next == LaptopForecastStatus.Stopped;
            case LaptopForecastStatus.Warming:
                return next == LaptopForecastStatus.Ready || next == LaptopForecastStatus.Stopped;
            case LaptopForecastStatus.Ready:
                switch (next)
                {
                    case LaptopForecastStatus.Draining:
                    case LaptopForecastStatus.Stopped:
                        return true;
                }
                return false;
            case LaptopForecastStatus.Draining:
                return next == LaptopForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(LaptopForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"laptop forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum - Cost, 2);
    }

    public LaptopForecast Copy()
    {
        return new LaptopForecast(Code, Title, Label, Maximum, Threshold, Pinned, Summary, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
