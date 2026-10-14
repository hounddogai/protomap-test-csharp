namespace Acme.Workforce.Engagement.Sentiment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment target is in its life cycle.</summary>
public enum SentimentTargetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One sentiment target, with what the planners need to act on it.</summary>
public sealed class SentimentTarget(string code, string title, string description, string checksum, string jobCode, string team, string summary, string contactEmail, int quantity, SentimentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Checksum { get; set; } = checksum;
    public string JobCode { get; set; } = jobCode;
    public string Team { get; set; } = team;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public int Quantity { get; set; } = quantity;
    public SentimentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment target in status draft.</summary>
    public static SentimentTarget NewSentimentTarget(string code, string title)
    {
        return new SentimentTarget(code, title, "", "", "", "", "", "", 0, SentimentTargetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentTargetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentTargetStatus next)
    {
        switch (Status)
        {
            case SentimentTargetStatus.Draft:
                return next == SentimentTargetStatus.Active;
            case SentimentTargetStatus.Pending:
                switch (next)
                {
                    case SentimentTargetStatus.Suspended:
                        return true;
                }
                return false;
            case SentimentTargetStatus.Active:
                switch (next)
                {
                    case SentimentTargetStatus.Suspended:
                        return true;
                }
                return false;
            case SentimentTargetStatus.Suspended:
                return next == SentimentTargetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SentimentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment target"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public SentimentTarget Copy()
    {
        return new SentimentTarget(
            Code,
            Title,
            Description,
            Checksum,
            JobCode,
            Team,
            Summary,
            ContactEmail,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
