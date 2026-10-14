namespace Acme.Workforce.Engagement.Sentiment.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment profile is in its life cycle.</summary>
public enum SentimentProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A sentiment profile as the sentiment team keeps it.</summary>
public sealed class SentimentProfile(string code, string name, string currency, string source, string externalRef, int leaveDays, string country, string reviewer, SentimentProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Currency { get; set; } = currency;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public SentimentProfileStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment profile in status draft.</summary>
    public static SentimentProfile NewSentimentProfile(string code, string name)
    {
        return new SentimentProfile(code, name, "", "", "", 0, "", "", SentimentProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentProfileStatus next)
    {
        switch (Status)
        {
            case SentimentProfileStatus.Draft:
                return next == SentimentProfileStatus.Closed;
            case SentimentProfileStatus.Pending:
                switch (next)
                {
                    case SentimentProfileStatus.Suspended:
                    case SentimentProfileStatus.Closed:
                        return true;
                }
                return false;
            case SentimentProfileStatus.Active:
                return next == SentimentProfileStatus.Suspended || next == SentimentProfileStatus.Closed;
            case SentimentProfileStatus.Suspended:
                return next == SentimentProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SentimentProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment profile"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public SentimentProfile Copy()
    {
        return new SentimentProfile(Code, Name, Currency, Source, ExternalRef, LeaveDays, Country, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
