namespace Acme.Workforce.Contractors.Engagement.EngagementSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling engagement forecast is in its life cycle.</summary>
public enum RollingEngagementForecastStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling engagement forecast and its current status.</summary>
public sealed class RollingEngagementForecast(string code, string label, int graceDays, double budget, string locale, int capacity, string summary, double weight, double threshold, RollingEngagementForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public double Budget { get; set; } = budget;
    public string Locale { get; set; } = locale;
    public int Capacity { get; set; } = capacity;
    public string Summary { get; set; } = summary;
    public double Weight { get; set; } = weight;
    public double Threshold { get; set; } = threshold;
    public RollingEngagementForecastStatus Status { get; set; } = status;

    /// <summary>Starts a rolling engagement forecast in status open.</summary>
    public static RollingEngagementForecast NewRollingEngagementForecast(string code, string label)
    {
        return new RollingEngagementForecast(
            code,
            label,
            0,
            0.0,
            "",
            0,
            "",
            0.0,
            0.0,
            RollingEngagementForecastStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingEngagementForecastStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingEngagementForecastStatus next)
    {
        switch (Status)
        {
            case RollingEngagementForecastStatus.Open:
                switch (next)
                {
                    case RollingEngagementForecastStatus.InReview:
                    case RollingEngagementForecastStatus.Archived:
                        return true;
                }
                return false;
            case RollingEngagementForecastStatus.InReview:
                return next == RollingEngagementForecastStatus.Resolved;
            case RollingEngagementForecastStatus.Resolved:
                return next == RollingEngagementForecastStatus.Reopened || next == RollingEngagementForecastStatus.Archived;
            case RollingEngagementForecastStatus.Reopened:
                return next == RollingEngagementForecastStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingEngagementForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling engagement forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays * (double)Capacity, 2);
    }

    public RollingEngagementForecast Copy()
    {
        return new RollingEngagementForecast(
            Code,
            Label,
            GraceDays,
            Budget,
            Locale,
            Capacity,
            Summary,
            Weight,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
