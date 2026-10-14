namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey milestone is in its life cycle.</summary>
public enum PulseSurveyMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pulse survey milestone. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyMilestone(string code, string label, double rating, double fte, double ceiling, int durationMinutes, string badge, string costCenter, int windowDays, double budget, PulseSurveyMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rating { get; set; } = rating;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public string CostCenter { get; set; } = costCenter;
    public int WindowDays { get; set; } = windowDays;
    public double Budget { get; set; } = budget;
    public PulseSurveyMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey milestone in status planned.</summary>
    public static PulseSurveyMilestone NewPulseSurveyMilestone(string code, string label)
    {
        return new PulseSurveyMilestone(
            code,
            label,
            0.0,
            0.0,
            0.0,
            0,
            "",
            "",
            0,
            0.0,
            PulseSurveyMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyMilestoneStatus next)
    {
        switch (Status)
        {
            case PulseSurveyMilestoneStatus.Planned:
                return next == PulseSurveyMilestoneStatus.Completed;
            case PulseSurveyMilestoneStatus.Scheduled:
                return next == PulseSurveyMilestoneStatus.Completed;
            case PulseSurveyMilestoneStatus.InProgress:
                return next == PulseSurveyMilestoneStatus.Cancelled;
            case PulseSurveyMilestoneStatus.Completed:
                return next == PulseSurveyMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PulseSurveyMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays - Rating, 2);
    }

    public PulseSurveyMilestone Copy()
    {
        return new PulseSurveyMilestone(
            Code,
            Label,
            Rating,
            Fte,
            Ceiling,
            DurationMinutes,
            Badge,
            CostCenter,
            WindowDays,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
