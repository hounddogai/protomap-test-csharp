namespace Acme.Workforce.Engagement.Sentiment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment ledger is in its life cycle.</summary>
public enum SentimentLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One sentiment ledger, with what the planners need to act on it.</summary>
public sealed class SentimentLedger(string code, string label, string badge, string country, double cost, SentimentLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public SentimentLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment ledger in status created.</summary>
    public static SentimentLedger NewSentimentLedger(string code, string label)
    {
        return new SentimentLedger(code, label, "", "", 0.0, SentimentLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentLedgerStatus next)
    {
        switch (Status)
        {
            case SentimentLedgerStatus.Created:
                return next == SentimentLedgerStatus.Validated || next == SentimentLedgerStatus.Published;
            case SentimentLedgerStatus.Validated:
                return next == SentimentLedgerStatus.Published;
            case SentimentLedgerStatus.Published:
                switch (next)
                {
                    case SentimentLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public SentimentLedger Copy()
    {
        return new SentimentLedger(Code, Label, Badge, Country, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
