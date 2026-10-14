namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment score is in its life cycle.</summary>
public enum SentimentScoreStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a sentiment score and its current status.</summary>
public sealed class SentimentScore(string code, string name, string timezone, double ratio, bool pinned, string badge, string jobCode, int revision, int priority, SentimentScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public string Badge { get; set; } = badge;
    public string JobCode { get; set; } = jobCode;
    public int Revision { get; set; } = revision;
    public int Priority { get; set; } = priority;
    public SentimentScoreStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment score in status requested.</summary>
    public static SentimentScore NewSentimentScore(string code, string name)
    {
        return new SentimentScore(code, name, "", 0.0, false, "", "", 0, 0, SentimentScoreStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentScoreStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentScoreStatus next)
    {
        switch (Status)
        {
            case SentimentScoreStatus.Requested:
                return next == SentimentScoreStatus.Granted;
            case SentimentScoreStatus.Granted:
                switch (next)
                {
                    case SentimentScoreStatus.Revoked:
                    case SentimentScoreStatus.Expired:
                        return true;
                }
                return false;
            case SentimentScoreStatus.Revoked:
                switch (next)
                {
                    case SentimentScoreStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"sentiment score"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision - (double)Priority, 2);
    }

    public SentimentScore Copy()
    {
        return new SentimentScore(Code, Name, Timezone, Ratio, Pinned, Badge, JobCode, Revision, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
