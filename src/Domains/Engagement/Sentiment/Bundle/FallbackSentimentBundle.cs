namespace Acme.Workforce.Engagement.Sentiment.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback sentiment bundle is in its life cycle.</summary>
public enum FallbackSentimentBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a fallback sentiment bundle and its current status.</summary>
public sealed class FallbackSentimentBundle(string code, string title, bool active, double weight, string grade, int version, FallbackSentimentBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public double Weight { get; set; } = weight;
    public string Grade { get; set; } = grade;
    public int Version { get; set; } = version;
    public FallbackSentimentBundleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback sentiment bundle in status open.</summary>
    public static FallbackSentimentBundle NewFallbackSentimentBundle(string code, string title)
    {
        return new FallbackSentimentBundle(code, title, false, 0.0, "", 0, FallbackSentimentBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSentimentBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSentimentBundleStatus next)
    {
        switch (Status)
        {
            case FallbackSentimentBundleStatus.Open:
                switch (next)
                {
                    case FallbackSentimentBundleStatus.Archived:
                        return true;
                }
                return false;
            case FallbackSentimentBundleStatus.InReview:
                return next == FallbackSentimentBundleStatus.Resolved || next == FallbackSentimentBundleStatus.Archived;
            case FallbackSentimentBundleStatus.Resolved:
                switch (next)
                {
                    case FallbackSentimentBundleStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackSentimentBundleStatus.Reopened:
                switch (next)
                {
                    case FallbackSentimentBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSentimentBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback sentiment bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Weight - (double)Version, 2);
    }

    public FallbackSentimentBundle Copy()
    {
        return new FallbackSentimentBundle(Code, Title, Active, Weight, Grade, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
