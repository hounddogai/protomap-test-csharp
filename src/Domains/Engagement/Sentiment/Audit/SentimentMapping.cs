namespace Acme.Workforce.Engagement.Sentiment.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment mapping is in its life cycle.</summary>
public enum SentimentMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A sentiment mapping as the sentiment team keeps it.</summary>
public sealed class SentimentMapping(string code, string title, string contactEmail, int graceDays, int updatedAt, SentimentMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int GraceDays { get; set; } = graceDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public SentimentMappingStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment mapping in status planned.</summary>
    public static SentimentMapping NewSentimentMapping(string code, string title)
    {
        return new SentimentMapping(code, title, "", 0, 0, SentimentMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentMappingStatus next)
    {
        switch (Status)
        {
            case SentimentMappingStatus.Planned:
                switch (next)
                {
                    case SentimentMappingStatus.Scheduled:
                    case SentimentMappingStatus.Completed:
                        return true;
                }
                return false;
            case SentimentMappingStatus.Scheduled:
                return next == SentimentMappingStatus.InProgress;
            case SentimentMappingStatus.InProgress:
                return next == SentimentMappingStatus.Cancelled;
            case SentimentMappingStatus.Completed:
                switch (next)
                {
                    case SentimentMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"sentiment mapping"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)UpdatedAt + (double)GraceDays, 2);
    }

    public SentimentMapping Copy()
    {
        return new SentimentMapping(Code, Title, ContactEmail, GraceDays, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
