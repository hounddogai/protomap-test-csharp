namespace Acme.Workforce.Engagement.Sentiment.SentimentVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary sentiment bundle is in its life cycle.</summary>
public enum PrimarySentimentBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One primary sentiment bundle, with what the planners need to act on it.</summary>
public sealed class PrimarySentimentBundle(string code, string title, int createdAt, string channel, double weight, bool active, PrimarySentimentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public PrimarySentimentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a primary sentiment bundle in status planned.</summary>
    public static PrimarySentimentBundle NewPrimarySentimentBundle(string code, string title)
    {
        return new PrimarySentimentBundle(code, title, 0, "", 0.0, false, PrimarySentimentBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySentimentBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySentimentBundleStatus next)
    {
        switch (Status)
        {
            case PrimarySentimentBundleStatus.Planned:
                return next == PrimarySentimentBundleStatus.InProgress || next == PrimarySentimentBundleStatus.Completed;
            case PrimarySentimentBundleStatus.Scheduled:
                switch (next)
                {
                    case PrimarySentimentBundleStatus.InProgress:
                    case PrimarySentimentBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimarySentimentBundleStatus.InProgress:
                switch (next)
                {
                    case PrimarySentimentBundleStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimarySentimentBundleStatus.Completed:
                return next == PrimarySentimentBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimarySentimentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary sentiment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Weight + (double)CreatedAt, 2);
    }

    public PrimarySentimentBundle Copy()
    {
        return new PrimarySentimentBundle(Code, Title, CreatedAt, Channel, Weight, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
