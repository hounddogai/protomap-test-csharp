namespace Acme.Workforce.Engagement.PulseSurvey.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pulse survey preference is in its life cycle.</summary>
public enum GlobalPulseSurveyPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A global pulse survey preference. Its code is unique within a tenant.</summary>
public sealed class GlobalPulseSurveyPreference(string code, string title, string source, string grade, double rate, string ownerId, bool pinned, double budget, GlobalPulseSurveyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Source { get; set; } = source;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public string OwnerId { get; set; } = ownerId;
    public bool Pinned { get; set; } = pinned;
    public double Budget { get; set; } = budget;
    public GlobalPulseSurveyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a global pulse survey preference in status requested.</summary>
    public static GlobalPulseSurveyPreference NewGlobalPulseSurveyPreference(string code, string title)
    {
        return new GlobalPulseSurveyPreference(
            code,
            title,
            "",
            "",
            0.0,
            "",
            false,
            0.0,
            GlobalPulseSurveyPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPulseSurveyPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPulseSurveyPreferenceStatus next)
    {
        switch (Status)
        {
            case GlobalPulseSurveyPreferenceStatus.Requested:
                return next == GlobalPulseSurveyPreferenceStatus.Granted;
            case GlobalPulseSurveyPreferenceStatus.Granted:
                return next == GlobalPulseSurveyPreferenceStatus.Revoked || next == GlobalPulseSurveyPreferenceStatus.Expired;
            case GlobalPulseSurveyPreferenceStatus.Revoked:
                return next == GlobalPulseSurveyPreferenceStatus.Expired;
        }
        return false;
    }

    public void MoveTo(GlobalPulseSurveyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pulse survey preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global pulse survey preference"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Budget - Rate, 2);
    }

    public GlobalPulseSurveyPreference Copy()
    {
        return new GlobalPulseSurveyPreference(Code, Title, Source, Grade, Rate, OwnerId, Pinned, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
