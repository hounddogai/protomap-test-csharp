namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey draft is in its life cycle.</summary>
public enum StrictPulseSurveyDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One strict pulse survey draft, with what the planners need to act on it.</summary>
public sealed class StrictPulseSurveyDraft(string code, string title, double score, double threshold, int tenureMonths, int attempts, int priority, int revision, string externalRef, StrictPulseSurveyDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Attempts { get; set; } = attempts;
    public int Priority { get; set; } = priority;
    public int Revision { get; set; } = revision;
    public string ExternalRef { get; set; } = externalRef;
    public StrictPulseSurveyDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey draft in status idle.</summary>
    public static StrictPulseSurveyDraft NewStrictPulseSurveyDraft(string code, string title)
    {
        return new StrictPulseSurveyDraft(code, title, 0.0, 0.0, 0, 0, 0, 0, "", StrictPulseSurveyDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyDraftStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyDraftStatus.Idle:
                return next == StrictPulseSurveyDraftStatus.Ready || next == StrictPulseSurveyDraftStatus.Stopped;
            case StrictPulseSurveyDraftStatus.Warming:
                return next == StrictPulseSurveyDraftStatus.Ready || next == StrictPulseSurveyDraftStatus.Draining;
            case StrictPulseSurveyDraftStatus.Ready:
                return next == StrictPulseSurveyDraftStatus.Draining;
            case StrictPulseSurveyDraftStatus.Draining:
                return next == StrictPulseSurveyDraftStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Threshold * (double)Attempts, 2);
    }

    public StrictPulseSurveyDraft Copy()
    {
        return new StrictPulseSurveyDraft(
            Code,
            Title,
            Score,
            Threshold,
            TenureMonths,
            Attempts,
            Priority,
            Revision,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
