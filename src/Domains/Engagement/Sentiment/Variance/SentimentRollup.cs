namespace Acme.Workforce.Engagement.Sentiment.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment rollup is in its life cycle.</summary>
public enum SentimentRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One sentiment rollup, with what the planners need to act on it.</summary>
public sealed class SentimentRollup(string code, string title, string badge, string channel, string timezone, double amount, SentimentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Badge { get; set; } = badge;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public double Amount { get; set; } = amount;
    public SentimentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment rollup in status proposed.</summary>
    public static SentimentRollup NewSentimentRollup(string code, string title)
    {
        return new SentimentRollup(code, title, "", "", "", 0.0, SentimentRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentRollupStatus next)
    {
        switch (Status)
        {
            case SentimentRollupStatus.Proposed:
                return next == SentimentRollupStatus.Rejected;
            case SentimentRollupStatus.Approved:
                switch (next)
                {
                    case SentimentRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case SentimentRollupStatus.Rejected:
                switch (next)
                {
                    case SentimentRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public SentimentRollup Copy()
    {
        return new SentimentRollup(Code, Title, Badge, Channel, Timezone, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
