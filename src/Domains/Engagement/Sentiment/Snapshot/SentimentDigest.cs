namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment digest is in its life cycle.</summary>
public enum SentimentDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A sentiment digest. Its code is unique within a tenant.</summary>
public sealed class SentimentDigest(string code, string title, int windowDays, string salaryBand, int sequence, Dictionary<string, string> attributes, string description, SentimentDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int WindowDays { get; set; } = windowDays;
    public string SalaryBand { get; set; } = salaryBand;
    public int Sequence { get; set; } = sequence;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Description { get; set; } = description;
    public SentimentDigestStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment digest in status requested.</summary>
    public static SentimentDigest NewSentimentDigest(string code, string title)
    {
        return new SentimentDigest(
            code,
            title,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            SentimentDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentDigestStatus next)
    {
        switch (Status)
        {
            case SentimentDigestStatus.Requested:
                switch (next)
                {
                    case SentimentDigestStatus.Expired:
                        return true;
                }
                return false;
            case SentimentDigestStatus.Granted:
                return next == SentimentDigestStatus.Revoked || next == SentimentDigestStatus.Expired;
            case SentimentDigestStatus.Revoked:
                return next == SentimentDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SentimentDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)Sequence - (double)WindowDays, 2);
    }

    public SentimentDigest Copy()
    {
        return new SentimentDigest(Code, Title, WindowDays, SalaryBand, Sequence, Attributes, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
