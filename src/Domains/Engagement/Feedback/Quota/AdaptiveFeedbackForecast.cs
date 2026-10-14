namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive feedback forecast is in its life cycle.</summary>
public enum AdaptiveFeedbackForecastStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adaptive feedback forecast and its current status.</summary>
public sealed class AdaptiveFeedbackForecast(string code, string title, int tenureMonths, double ceiling, int priority, int limit, AdaptiveFeedbackForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Ceiling { get; set; } = ceiling;
    public int Priority { get; set; } = priority;
    public int Limit { get; set; } = limit;
    public AdaptiveFeedbackForecastStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive feedback forecast in status new.</summary>
    public static AdaptiveFeedbackForecast NewAdaptiveFeedbackForecast(string code, string title)
    {
        return new AdaptiveFeedbackForecast(code, title, 0, 0.0, 0, 0, AdaptiveFeedbackForecastStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFeedbackForecastStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFeedbackForecastStatus next)
    {
        switch (Status)
        {
            case AdaptiveFeedbackForecastStatus.New:
                switch (next)
                {
                    case AdaptiveFeedbackForecastStatus.Queued:
                    case AdaptiveFeedbackForecastStatus.Running:
                        return true;
                }
                return false;
            case AdaptiveFeedbackForecastStatus.Queued:
                switch (next)
                {
                    case AdaptiveFeedbackForecastStatus.Done:
                        return true;
                }
                return false;
            case AdaptiveFeedbackForecastStatus.Running:
                return next == AdaptiveFeedbackForecastStatus.Failed;
            case AdaptiveFeedbackForecastStatus.Done:
                switch (next)
                {
                    case AdaptiveFeedbackForecastStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveFeedbackForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive feedback forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit - (double)Priority, 2);
    }

    public AdaptiveFeedbackForecast Copy()
    {
        return new AdaptiveFeedbackForecast(Code, Title, TenureMonths, Ceiling, Priority, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
