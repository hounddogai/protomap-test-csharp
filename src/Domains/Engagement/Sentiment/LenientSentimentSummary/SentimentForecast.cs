namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment forecast is in its life cycle.</summary>
public enum SentimentForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A sentiment forecast. Its code is unique within a tenant.</summary>
public sealed class SentimentForecast(string code, string title, string label, bool approved, bool active, int createdAt, SentimentForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public bool Active { get; set; } = active;
    public int CreatedAt { get; set; } = createdAt;
    public SentimentForecastStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment forecast in status draft.</summary>
    public static SentimentForecast NewSentimentForecast(string code, string title)
    {
        return new SentimentForecast(code, title, "", false, false, 0, SentimentForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentForecastStatus next)
    {
        switch (Status)
        {
            case SentimentForecastStatus.Draft:
                switch (next)
                {
                    case SentimentForecastStatus.Pending:
                        return true;
                }
                return false;
            case SentimentForecastStatus.Pending:
                switch (next)
                {
                    case SentimentForecastStatus.Suspended:
                        return true;
                }
                return false;
            case SentimentForecastStatus.Active:
                return next == SentimentForecastStatus.Suspended;
            case SentimentForecastStatus.Suspended:
                return next == SentimentForecastStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SentimentForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"sentiment forecast"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public SentimentForecast Copy()
    {
        return new SentimentForecast(Code, Title, Label, Approved, Active, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
