namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey target is in its life cycle.</summary>
public enum PulseSurveyTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pulse survey target, with what the planners need to act on it.</summary>
public sealed class PulseSurveyTarget(string code, string title, bool active, string employeeId, double cost, PulseSurveyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Active { get; set; } = active;
    public string EmployeeId { get; set; } = employeeId;
    public double Cost { get; set; } = cost;
    public PulseSurveyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey target in status proposed.</summary>
    public static PulseSurveyTarget NewPulseSurveyTarget(string code, string title)
    {
        return new PulseSurveyTarget(code, title, false, "", 0.0, PulseSurveyTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyTargetStatus next)
    {
        switch (Status)
        {
            case PulseSurveyTargetStatus.Proposed:
                switch (next)
                {
                    case PulseSurveyTargetStatus.Approved:
                        return true;
                }
                return false;
            case PulseSurveyTargetStatus.Approved:
                return next == PulseSurveyTargetStatus.Rejected || next == PulseSurveyTargetStatus.Withdrawn;
            case PulseSurveyTargetStatus.Rejected:
                switch (next)
                {
                    case PulseSurveyTargetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public PulseSurveyTarget Copy()
    {
        return new PulseSurveyTarget(Code, Title, Active, EmployeeId, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
