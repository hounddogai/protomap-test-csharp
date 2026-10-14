namespace Acme.Workforce.Engagement.Sentiment.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment preference is in its life cycle.</summary>
public enum SentimentPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One sentiment preference, with what the planners need to act on it.</summary>
public sealed class SentimentPreference(string code, string title, string channel, int quantity, string externalRef, string team, SentimentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public int Quantity { get; set; } = quantity;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public SentimentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment preference in status draft.</summary>
    public static SentimentPreference NewSentimentPreference(string code, string title)
    {
        return new SentimentPreference(code, title, "", 0, "", "", SentimentPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentPreferenceStatus next)
    {
        switch (Status)
        {
            case SentimentPreferenceStatus.Draft:
                return next == SentimentPreferenceStatus.Active || next == SentimentPreferenceStatus.Suspended;
            case SentimentPreferenceStatus.Pending:
                return next == SentimentPreferenceStatus.Active || next == SentimentPreferenceStatus.Closed;
            case SentimentPreferenceStatus.Active:
                return next == SentimentPreferenceStatus.Suspended || next == SentimentPreferenceStatus.Closed;
            case SentimentPreferenceStatus.Suspended:
                switch (next)
                {
                    case SentimentPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"sentiment preference"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public SentimentPreference Copy()
    {
        return new SentimentPreference(Code, Title, Channel, Quantity, ExternalRef, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
