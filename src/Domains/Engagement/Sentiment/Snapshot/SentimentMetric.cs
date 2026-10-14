namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment metric is in its life cycle.</summary>
public enum SentimentMetricStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a sentiment metric and its current status.</summary>
public sealed class SentimentMetric(string code, string name, int updatedAt, double maximum, int quantity, int attempts, string location, int createdAt, SentimentMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public int Attempts { get; set; } = attempts;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public SentimentMetricStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment metric in status requested.</summary>
    public static SentimentMetric NewSentimentMetric(string code, string name)
    {
        return new SentimentMetric(code, name, 0, 0.0, 0, 0, "", 0, SentimentMetricStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentMetricStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentMetricStatus next)
    {
        switch (Status)
        {
            case SentimentMetricStatus.Requested:
                return next == SentimentMetricStatus.Expired;
            case SentimentMetricStatus.Granted:
                switch (next)
                {
                    case SentimentMetricStatus.Revoked:
                    case SentimentMetricStatus.Expired:
                        return true;
                }
                return false;
            case SentimentMetricStatus.Revoked:
                switch (next)
                {
                    case SentimentMetricStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"sentiment metric"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Attempts * (double)Quantity, 2);
    }

    public SentimentMetric Copy()
    {
        return new SentimentMetric(Code, Name, UpdatedAt, Maximum, Quantity, Attempts, Location, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
