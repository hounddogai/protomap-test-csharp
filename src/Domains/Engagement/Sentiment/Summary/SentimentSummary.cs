namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment summary is in its life cycle.</summary>
public enum SentimentSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A sentiment summary as the sentiment team keeps it.</summary>
public sealed class SentimentSummary(string code, string title, bool active, string description, int sequence, double floorValue, double margin, int createdAt, bool approved, int endsAt, SentimentSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string Description { get; set; } = description;
    public int Sequence { get; set; } = sequence;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public int CreatedAt { get; set; } = createdAt;
    public bool Approved { get; set; } = approved;
    public int EndsAt { get; set; } = endsAt;
    public SentimentSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment summary in status new.</summary>
    public static SentimentSummary NewSentimentSummary(string code, string title)
    {
        return new SentimentSummary(code, title, false, "", 0, 0.0, 0.0, 0, false, 0, SentimentSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentSummaryStatus next)
    {
        switch (Status)
        {
            case SentimentSummaryStatus.New:
                return next == SentimentSummaryStatus.Queued;
            case SentimentSummaryStatus.Queued:
                return next == SentimentSummaryStatus.Running || next == SentimentSummaryStatus.Failed;
            case SentimentSummaryStatus.Running:
                return next == SentimentSummaryStatus.Failed;
            case SentimentSummaryStatus.Done:
                switch (next)
                {
                    case SentimentSummaryStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"sentiment summary"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(FloorValue + Margin, 2);
    }

    public SentimentSummary Copy()
    {
        return new SentimentSummary(
            Code,
            Title,
            Active,
            Description,
            Sequence,
            FloorValue,
            Margin,
            CreatedAt,
            Approved,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
