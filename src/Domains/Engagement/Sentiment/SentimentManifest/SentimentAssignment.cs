namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment assignment is in its life cycle.</summary>
public enum SentimentAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One sentiment assignment, with what the planners need to act on it.</summary>
public sealed class SentimentAssignment(string code, string title, int batchSize, Dictionary<string, string> attributes, int tenureMonths, SentimentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int TenureMonths { get; set; } = tenureMonths;
    public SentimentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment assignment in status created.</summary>
    public static SentimentAssignment NewSentimentAssignment(string code, string title)
    {
        return new SentimentAssignment(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0,
            SentimentAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentAssignmentStatus next)
    {
        switch (Status)
        {
            case SentimentAssignmentStatus.Created:
                switch (next)
                {
                    case SentimentAssignmentStatus.Published:
                    case SentimentAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case SentimentAssignmentStatus.Validated:
                return next == SentimentAssignmentStatus.Published;
            case SentimentAssignmentStatus.Published:
                switch (next)
                {
                    case SentimentAssignmentStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment assignment"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round((double)TenureMonths * (double)BatchSize, 2);
    }

    public SentimentAssignment Copy()
    {
        return new SentimentAssignment(Code, Title, BatchSize, Attributes, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
