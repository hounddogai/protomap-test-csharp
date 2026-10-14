namespace Acme.Workforce.Engagement.Sentiment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending sentiment digest is in its life cycle.</summary>
public enum PendingSentimentDigestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pending sentiment digest. Its code is unique within a tenant.</summary>
public sealed class PendingSentimentDigest(string code, string name, string segment, int endsAt, string description, string summary, string phone, string channel, List<string> tags, double score, PendingSentimentDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int EndsAt { get; set; } = endsAt;
    public string Description { get; set; } = description;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public string Channel { get; set; } = channel;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public PendingSentimentDigestStatus Status { get; set; } = status;

    /// <summary>Starts a pending sentiment digest in status draft.</summary>
    public static PendingSentimentDigest NewPendingSentimentDigest(string code, string name)
    {
        return new PendingSentimentDigest(
            code,
            name,
            "",
            0,
            "",
            "",
            "",
            "",
            new List<string>(),
            0.0,
            PendingSentimentDigestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSentimentDigestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSentimentDigestStatus next)
    {
        switch (Status)
        {
            case PendingSentimentDigestStatus.Draft:
                switch (next)
                {
                    case PendingSentimentDigestStatus.Suspended:
                    case PendingSentimentDigestStatus.Closed:
                        return true;
                }
                return false;
            case PendingSentimentDigestStatus.Pending:
                return next == PendingSentimentDigestStatus.Active || next == PendingSentimentDigestStatus.Closed;
            case PendingSentimentDigestStatus.Active:
                switch (next)
                {
                    case PendingSentimentDigestStatus.Suspended:
                    case PendingSentimentDigestStatus.Closed:
                        return true;
                }
                return false;
            case PendingSentimentDigestStatus.Suspended:
                return next == PendingSentimentDigestStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PendingSentimentDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending sentiment digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedExposure()
    {
        return Math.Round(Score - (double)EndsAt, 2);
    }

    public PendingSentimentDigest Copy()
    {
        return new PendingSentimentDigest(
            Code,
            Name,
            Segment,
            EndsAt,
            Description,
            Summary,
            Phone,
            Channel,
            Tags,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
