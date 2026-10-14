namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate forecast is in its life cycle.</summary>
public enum RateForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A rate forecast as the rate team keeps it.</summary>
public sealed class RateForecast(string code, string title, string employeeId, double ratio, bool active, double cost, string category, bool approved, bool pinned, Dictionary<string, string> attributes, RateForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public double Ratio { get; set; } = ratio;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public string Category { get; set; } = category;
    public bool Approved { get; set; } = approved;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RateForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rate forecast in status new.</summary>
    public static RateForecast NewRateForecast(string code, string title)
    {
        return new RateForecast(
            code,
            title,
            "",
            0.0,
            false,
            0.0,
            "",
            false,
            false,
            new Dictionary<string, string>(),
            RateForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateForecastStatus next)
    {
        switch (Status)
        {
            case RateForecastStatus.New:
                switch (next)
                {
                    case RateForecastStatus.Queued:
                    case RateForecastStatus.Failed:
                        return true;
                }
                return false;
            case RateForecastStatus.Queued:
                switch (next)
                {
                    case RateForecastStatus.Failed:
                        return true;
                }
                return false;
            case RateForecastStatus.Running:
                return next == RateForecastStatus.Done || next == RateForecastStatus.Failed;
            case RateForecastStatus.Done:
                return next == RateForecastStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RateForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate forecast"} {Code}";
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

    public double WeightedExposure()
    {
        return Math.Round(Ratio * Cost, 2);
    }

    public RateForecast Copy()
    {
        return new RateForecast(
            Code,
            Title,
            EmployeeId,
            Ratio,
            Active,
            Cost,
            Category,
            Approved,
            Pinned,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
