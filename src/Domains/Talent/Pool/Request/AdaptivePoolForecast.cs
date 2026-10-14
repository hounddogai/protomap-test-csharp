namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive pool forecast is in its life cycle.</summary>
public enum AdaptivePoolForecastStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a adaptive pool forecast and its current status.</summary>
public sealed class AdaptivePoolForecast(string code, string title, string managerId, int batchSize, string label, int tenureMonths, AdaptivePoolForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int BatchSize { get; set; } = batchSize;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public AdaptivePoolForecastStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive pool forecast in status idle.</summary>
    public static AdaptivePoolForecast NewAdaptivePoolForecast(string code, string title)
    {
        return new AdaptivePoolForecast(code, title, "", 0, "", 0, AdaptivePoolForecastStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptivePoolForecastStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptivePoolForecastStatus next)
    {
        switch (Status)
        {
            case AdaptivePoolForecastStatus.Idle:
                switch (next)
                {
                    case AdaptivePoolForecastStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptivePoolForecastStatus.Warming:
                return next == AdaptivePoolForecastStatus.Draining || next == AdaptivePoolForecastStatus.Stopped;
            case AdaptivePoolForecastStatus.Ready:
                switch (next)
                {
                    case AdaptivePoolForecastStatus.Draining:
                    case AdaptivePoolForecastStatus.Stopped:
                        return true;
                }
                return false;
            case AdaptivePoolForecastStatus.Draining:
                switch (next)
                {
                    case AdaptivePoolForecastStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptivePoolForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive pool forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)TenureMonths - (double)BatchSize, 2);
    }

    public AdaptivePoolForecast Copy()
    {
        return new AdaptivePoolForecast(Code, Title, ManagerId, BatchSize, Label, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
