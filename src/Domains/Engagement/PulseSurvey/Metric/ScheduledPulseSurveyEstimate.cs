namespace Acme.Workforce.Engagement.PulseSurvey.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pulse survey estimate is in its life cycle.</summary>
public enum ScheduledPulseSurveyEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A scheduled pulse survey estimate as the pulse survey team keeps it.</summary>
public sealed class ScheduledPulseSurveyEstimate(string code, string title, string skill, int attempts, bool active, double ceiling, ScheduledPulseSurveyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public double Ceiling { get; set; } = ceiling;
    public ScheduledPulseSurveyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pulse survey estimate in status open.</summary>
    public static ScheduledPulseSurveyEstimate NewScheduledPulseSurveyEstimate(string code, string title)
    {
        return new ScheduledPulseSurveyEstimate(
            code,
            title,
            "",
            0,
            false,
            0.0,
            ScheduledPulseSurveyEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPulseSurveyEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPulseSurveyEstimateStatus next)
    {
        switch (Status)
        {
            case ScheduledPulseSurveyEstimateStatus.Open:
                return next == ScheduledPulseSurveyEstimateStatus.Reopened || next == ScheduledPulseSurveyEstimateStatus.Archived;
            case ScheduledPulseSurveyEstimateStatus.InReview:
                return next == ScheduledPulseSurveyEstimateStatus.Resolved;
            case ScheduledPulseSurveyEstimateStatus.Resolved:
                return next == ScheduledPulseSurveyEstimateStatus.Reopened || next == ScheduledPulseSurveyEstimateStatus.Archived;
            case ScheduledPulseSurveyEstimateStatus.Reopened:
                switch (next)
                {
                    case ScheduledPulseSurveyEstimateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledPulseSurveyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pulse survey estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ceiling + (double)Attempts, 2);
    }

    public ScheduledPulseSurveyEstimate Copy()
    {
        return new ScheduledPulseSurveyEstimate(Code, Title, Skill, Attempts, Active, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
