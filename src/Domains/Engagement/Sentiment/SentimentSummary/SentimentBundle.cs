namespace Acme.Workforce.Engagement.Sentiment.SentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment bundle is in its life cycle.</summary>
public enum SentimentBundleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a sentiment bundle and its current status.</summary>
public sealed class SentimentBundle(string code, string name, string category, bool visible, string reviewer, string team, double amount, SentimentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Category { get; set; } = category;
    public bool Visible { get; set; } = visible;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public SentimentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment bundle in status created.</summary>
    public static SentimentBundle NewSentimentBundle(string code, string name)
    {
        return new SentimentBundle(code, name, "", false, "", "", 0.0, SentimentBundleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentBundleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentBundleStatus next)
    {
        switch (Status)
        {
            case SentimentBundleStatus.Created:
                return next == SentimentBundleStatus.Validated || next == SentimentBundleStatus.Retired;
            case SentimentBundleStatus.Validated:
                switch (next)
                {
                    case SentimentBundleStatus.Published:
                        return true;
                }
                return false;
            case SentimentBundleStatus.Published:
                return next == SentimentBundleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SentimentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public SentimentBundle Copy()
    {
        return new SentimentBundle(Code, Name, Category, Visible, Reviewer, Team, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
