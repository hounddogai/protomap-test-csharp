namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey variance is in its life cycle.</summary>
public enum StrictPulseSurveyVarianceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One strict pulse survey variance, with what the planners need to act on it.</summary>
public sealed class StrictPulseSurveyVariance(string code, string title, bool approved, int startsAt, int expiresAt, double amount, bool archived, int capacity, StrictPulseSurveyVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Amount { get; set; } = amount;
    public bool Archived { get; set; } = archived;
    public int Capacity { get; set; } = capacity;
    public StrictPulseSurveyVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey variance in status open.</summary>
    public static StrictPulseSurveyVariance NewStrictPulseSurveyVariance(string code, string title)
    {
        return new StrictPulseSurveyVariance(
            code,
            title,
            false,
            0,
            0,
            0.0,
            false,
            0,
            StrictPulseSurveyVarianceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyVarianceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyVarianceStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyVarianceStatus.Open:
                switch (next)
                {
                    case StrictPulseSurveyVarianceStatus.Reopened:
                    case StrictPulseSurveyVarianceStatus.Archived:
                        return true;
                }
                return false;
            case StrictPulseSurveyVarianceStatus.InReview:
                switch (next)
                {
                    case StrictPulseSurveyVarianceStatus.Reopened:
                        return true;
                }
                return false;
            case StrictPulseSurveyVarianceStatus.Resolved:
                switch (next)
                {
                    case StrictPulseSurveyVarianceStatus.Reopened:
                        return true;
                }
                return false;
            case StrictPulseSurveyVarianceStatus.Reopened:
                return next == StrictPulseSurveyVarianceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)ExpiresAt * (double)StartsAt, 2);
    }

    public StrictPulseSurveyVariance Copy()
    {
        return new StrictPulseSurveyVariance(
            Code,
            Title,
            Approved,
            StartsAt,
            ExpiresAt,
            Amount,
            Archived,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
