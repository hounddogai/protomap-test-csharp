namespace Acme.Workforce.Engagement.Feedback.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive feedback preference is in its life cycle.</summary>
public enum AdaptiveFeedbackPreferenceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a adaptive feedback preference and its current status.</summary>
public sealed class AdaptiveFeedbackPreference(string code, string name, string summary, double score, string team, double margin, int attempts, AdaptiveFeedbackPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Score { get; set; } = score;
    public string Team { get; set; } = team;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public AdaptiveFeedbackPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive feedback preference in status created.</summary>
    public static AdaptiveFeedbackPreference NewAdaptiveFeedbackPreference(string code, string name)
    {
        return new AdaptiveFeedbackPreference(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            0,
            AdaptiveFeedbackPreferenceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveFeedbackPreferenceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveFeedbackPreferenceStatus next)
    {
        switch (Status)
        {
            case AdaptiveFeedbackPreferenceStatus.Created:
                switch (next)
                {
                    case AdaptiveFeedbackPreferenceStatus.Published:
                    case AdaptiveFeedbackPreferenceStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveFeedbackPreferenceStatus.Validated:
                return next == AdaptiveFeedbackPreferenceStatus.Published || next == AdaptiveFeedbackPreferenceStatus.Retired;
            case AdaptiveFeedbackPreferenceStatus.Published:
                return next == AdaptiveFeedbackPreferenceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveFeedbackPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive feedback preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Attempts + Margin, 2);
    }

    public AdaptiveFeedbackPreference Copy()
    {
        return new AdaptiveFeedbackPreference(Code, Name, Summary, Score, Team, Margin, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
