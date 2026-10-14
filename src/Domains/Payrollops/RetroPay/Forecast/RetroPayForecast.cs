namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay forecast is in its life cycle.</summary>
public enum RetroPayForecastStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One retro pay forecast, with what the planners need to act on it.</summary>
public sealed class RetroPayForecast(string code, string title, string description, Dictionary<string, string> attributes, int leaveDays, double ceiling, string region, int quantity, RetroPayForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int LeaveDays { get; set; } = leaveDays;
    public double Ceiling { get; set; } = ceiling;
    public string Region { get; set; } = region;
    public int Quantity { get; set; } = quantity;
    public RetroPayForecastStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay forecast in status created.</summary>
    public static RetroPayForecast NewRetroPayForecast(string code, string title)
    {
        return new RetroPayForecast(
            code,
            title,
            "",
            new Dictionary<string, string>(),
            0,
            0.0,
            "",
            0,
            RetroPayForecastStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayForecastStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayForecastStatus next)
    {
        switch (Status)
        {
            case RetroPayForecastStatus.Created:
                return next == RetroPayForecastStatus.Published || next == RetroPayForecastStatus.Retired;
            case RetroPayForecastStatus.Validated:
                return next == RetroPayForecastStatus.Published || next == RetroPayForecastStatus.Retired;
            case RetroPayForecastStatus.Published:
                return next == RetroPayForecastStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RetroPayForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"retro pay forecast"} {Code}";
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

    public double NetTotal()
    {
        return Math.Round(Ceiling + (double)LeaveDays, 2);
    }

    public RetroPayForecast Copy()
    {
        return new RetroPayForecast(
            Code,
            Title,
            Description,
            Attributes,
            LeaveDays,
            Ceiling,
            Region,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
