namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered feedback preference is in its life cycle.</summary>
public enum TieredFeedbackPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One tiered feedback preference, with what the planners need to act on it.</summary>
public sealed class TieredFeedbackPreference(string code, string name, string ownerId, double rating, string timezone, string locale, int retries, bool archived, int durationMinutes, string category, TieredFeedbackPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string OwnerId { get; set; } = ownerId;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public string Locale { get; set; } = locale;
    public int Retries { get; set; } = retries;
    public bool Archived { get; set; } = archived;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Category { get; set; } = category;
    public TieredFeedbackPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered feedback preference in status open.</summary>
    public static TieredFeedbackPreference NewTieredFeedbackPreference(string code, string name)
    {
        return new TieredFeedbackPreference(
            code,
            name,
            "",
            0.0,
            "",
            "",
            0,
            false,
            0,
            "",
            TieredFeedbackPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredFeedbackPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredFeedbackPreferenceStatus next)
    {
        switch (Status)
        {
            case TieredFeedbackPreferenceStatus.Open:
                switch (next)
                {
                    case TieredFeedbackPreferenceStatus.InReview:
                    case TieredFeedbackPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case TieredFeedbackPreferenceStatus.InReview:
                return next == TieredFeedbackPreferenceStatus.Archived;
            case TieredFeedbackPreferenceStatus.Resolved:
                switch (next)
                {
                    case TieredFeedbackPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case TieredFeedbackPreferenceStatus.Reopened:
                switch (next)
                {
                    case TieredFeedbackPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredFeedbackPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered feedback preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Rating - (double)Retries, 2);
    }

    public TieredFeedbackPreference Copy()
    {
        return new TieredFeedbackPreference(
            Code,
            Name,
            OwnerId,
            Rating,
            Timezone,
            Locale,
            Retries,
            Archived,
            DurationMinutes,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
