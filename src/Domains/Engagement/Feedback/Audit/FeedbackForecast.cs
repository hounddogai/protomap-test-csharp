namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback forecast is in its life cycle.</summary>
public enum FeedbackForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback forecast. Its code is unique within a tenant.</summary>
public sealed class FeedbackForecast(string code, string label, int durationMinutes, double rate, double ceiling, string description, FeedbackForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rate { get; set; } = rate;
    public double Ceiling { get; set; } = ceiling;
    public string Description { get; set; } = description;
    public FeedbackForecastStatus Status { get; set; } = status;

    /// <summary>Starts a feedback forecast in status draft.</summary>
    public static FeedbackForecast NewFeedbackForecast(string code, string label)
    {
        return new FeedbackForecast(code, label, 0, 0.0, 0.0, "", FeedbackForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackForecastStatus next)
    {
        switch (Status)
        {
            case FeedbackForecastStatus.Draft:
                switch (next)
                {
                    case FeedbackForecastStatus.Closed:
                        return true;
                }
                return false;
            case FeedbackForecastStatus.Pending:
                return next == FeedbackForecastStatus.Suspended;
            case FeedbackForecastStatus.Active:
                return next == FeedbackForecastStatus.Suspended || next == FeedbackForecastStatus.Closed;
            case FeedbackForecastStatus.Suspended:
                return next == FeedbackForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetValue()
    {
        return Math.Round(Rate * Ceiling, 2);
    }

    public FeedbackForecast Copy()
    {
        return new FeedbackForecast(Code, Label, DurationMinutes, Rate, Ceiling, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
