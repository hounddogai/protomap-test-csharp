namespace Acme.Workforce.Assets.Warranty.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty forecast is in its life cycle.</summary>
public enum WarrantyForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One warranty forecast, with what the planners need to act on it.</summary>
public sealed class WarrantyForecast(string code, string title, double hours, int retries, string jobCode, Dictionary<string, string> attributes, double ceiling, int endsAt, string managerId, double rate, WarrantyForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public string ManagerId { get; set; } = managerId;
    public double Rate { get; set; } = rate;
    public WarrantyForecastStatus Status { get; set; } = status;

    /// <summary>Starts a warranty forecast in status idle.</summary>
    public static WarrantyForecast NewWarrantyForecast(string code, string title)
    {
        return new WarrantyForecast(
            code,
            title,
            0.0,
            0,
            "",
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            0.0,
            WarrantyForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyForecastStatus next)
    {
        switch (Status)
        {
            case WarrantyForecastStatus.Idle:
                return next == WarrantyForecastStatus.Draining;
            case WarrantyForecastStatus.Warming:
                return next == WarrantyForecastStatus.Ready || next == WarrantyForecastStatus.Stopped;
            case WarrantyForecastStatus.Ready:
                return next == WarrantyForecastStatus.Stopped;
            case WarrantyForecastStatus.Draining:
                return next == WarrantyForecastStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WarrantyForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedVolume()
    {
        return Math.Round(Rate + Hours, 2);
    }

    public WarrantyForecast Copy()
    {
        return new WarrantyForecast(
            Code,
            Title,
            Hours,
            Retries,
            JobCode,
            Attributes,
            Ceiling,
            EndsAt,
            ManagerId,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
