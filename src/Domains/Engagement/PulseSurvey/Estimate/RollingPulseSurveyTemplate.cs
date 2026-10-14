namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pulse survey template is in its life cycle.</summary>
public enum RollingPulseSurveyTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a rolling pulse survey template and its current status.</summary>
public sealed class RollingPulseSurveyTemplate(string code, string title, double discount, string team, string managerId, RollingPulseSurveyTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public RollingPulseSurveyTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pulse survey template in status planned.</summary>
    public static RollingPulseSurveyTemplate NewRollingPulseSurveyTemplate(string code, string title)
    {
        return new RollingPulseSurveyTemplate(code, title, 0.0, "", "", RollingPulseSurveyTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPulseSurveyTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPulseSurveyTemplateStatus next)
    {
        switch (Status)
        {
            case RollingPulseSurveyTemplateStatus.Planned:
                switch (next)
                {
                    case RollingPulseSurveyTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case RollingPulseSurveyTemplateStatus.Scheduled:
                switch (next)
                {
                    case RollingPulseSurveyTemplateStatus.Completed:
                    case RollingPulseSurveyTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingPulseSurveyTemplateStatus.InProgress:
                switch (next)
                {
                    case RollingPulseSurveyTemplateStatus.Completed:
                    case RollingPulseSurveyTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case RollingPulseSurveyTemplateStatus.Completed:
                switch (next)
                {
                    case RollingPulseSurveyTemplateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPulseSurveyTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pulse survey template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling pulse survey template"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public RollingPulseSurveyTemplate Copy()
    {
        return new RollingPulseSurveyTemplate(Code, Title, Discount, Team, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
