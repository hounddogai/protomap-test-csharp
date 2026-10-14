namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback sentiment profile is in its life cycle.</summary>
public enum FallbackSentimentProfileStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback sentiment profile. Its code is unique within a tenant.</summary>
public sealed class FallbackSentimentProfile(string code, string title, int attempts, int priority, int batchSize, List<string> tags, string notes, FallbackSentimentProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public string Notes { get; set; } = notes;
    public FallbackSentimentProfileStatus Status { get; set; } = status;

    /// <summary>Starts a fallback sentiment profile in status proposed.</summary>
    public static FallbackSentimentProfile NewFallbackSentimentProfile(string code, string title)
    {
        return new FallbackSentimentProfile(
            code,
            title,
            0,
            0,
            0,
            new List<string>(),
            "",
            FallbackSentimentProfileStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSentimentProfileStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSentimentProfileStatus next)
    {
        switch (Status)
        {
            case FallbackSentimentProfileStatus.Proposed:
                switch (next)
                {
                    case FallbackSentimentProfileStatus.Approved:
                    case FallbackSentimentProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackSentimentProfileStatus.Approved:
                switch (next)
                {
                    case FallbackSentimentProfileStatus.Rejected:
                    case FallbackSentimentProfileStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackSentimentProfileStatus.Rejected:
                switch (next)
                {
                    case FallbackSentimentProfileStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSentimentProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback sentiment profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback sentiment profile"} {Code}";
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

    public double EffectiveVolume()
    {
        return Math.Round((double)Priority * (double)Attempts, 2);
    }

    public FallbackSentimentProfile Copy()
    {
        return new FallbackSentimentProfile(Code, Title, Attempts, Priority, BatchSize, Tags, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
