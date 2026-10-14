namespace Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback sentiment preference is in its life cycle.</summary>
public enum FallbackSentimentPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback sentiment preference. Its code is unique within a tenant.</summary>
public sealed class FallbackSentimentPreference(string code, string title, string currency, double cost, bool active, FallbackSentimentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public FallbackSentimentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a fallback sentiment preference in status idle.</summary>
    public static FallbackSentimentPreference NewFallbackSentimentPreference(string code, string title)
    {
        return new FallbackSentimentPreference(code, title, "", 0.0, false, FallbackSentimentPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackSentimentPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackSentimentPreferenceStatus next)
    {
        switch (Status)
        {
            case FallbackSentimentPreferenceStatus.Idle:
                switch (next)
                {
                    case FallbackSentimentPreferenceStatus.Ready:
                        return true;
                }
                return false;
            case FallbackSentimentPreferenceStatus.Warming:
                return next == FallbackSentimentPreferenceStatus.Stopped;
            case FallbackSentimentPreferenceStatus.Ready:
                switch (next)
                {
                    case FallbackSentimentPreferenceStatus.Draining:
                    case FallbackSentimentPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackSentimentPreferenceStatus.Draining:
                switch (next)
                {
                    case FallbackSentimentPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackSentimentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback sentiment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback sentiment preference"} {Code}";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public FallbackSentimentPreference Copy()
    {
        return new FallbackSentimentPreference(Code, Title, Currency, Cost, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
