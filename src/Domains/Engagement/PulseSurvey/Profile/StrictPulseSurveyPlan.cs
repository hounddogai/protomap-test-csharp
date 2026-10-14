namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey plan is in its life cycle.</summary>
public enum StrictPulseSurveyPlanStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One strict pulse survey plan, with what the planners need to act on it.</summary>
public sealed class StrictPulseSurveyPlan(string code, string label, string skill, int createdAt, int quantity, StrictPulseSurveyPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int CreatedAt { get; set; } = createdAt;
    public int Quantity { get; set; } = quantity;
    public StrictPulseSurveyPlanStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey plan in status proposed.</summary>
    public static StrictPulseSurveyPlan NewStrictPulseSurveyPlan(string code, string label)
    {
        return new StrictPulseSurveyPlan(code, label, "", 0, 0, StrictPulseSurveyPlanStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyPlanStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyPlanStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyPlanStatus.Proposed:
                switch (next)
                {
                    case StrictPulseSurveyPlanStatus.Rejected:
                        return true;
                }
                return false;
            case StrictPulseSurveyPlanStatus.Approved:
                switch (next)
                {
                    case StrictPulseSurveyPlanStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictPulseSurveyPlanStatus.Rejected:
                return next == StrictPulseSurveyPlanStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)CreatedAt * (double)Quantity, 2);
    }

    public StrictPulseSurveyPlan Copy()
    {
        return new StrictPulseSurveyPlan(Code, Label, Skill, CreatedAt, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
