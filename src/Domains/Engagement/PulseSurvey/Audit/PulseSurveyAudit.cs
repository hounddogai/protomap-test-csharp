namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey audit is in its life cycle.</summary>
public enum PulseSurveyAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One pulse survey audit, with what the planners need to act on it.</summary>
public sealed class PulseSurveyAudit(string code, string label, string jobCode, int windowDays, string category, bool active, PulseSurveyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public PulseSurveyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey audit in status draft.</summary>
    public static PulseSurveyAudit NewPulseSurveyAudit(string code, string label)
    {
        return new PulseSurveyAudit(code, label, "", 0, "", false, PulseSurveyAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyAuditStatus next)
    {
        switch (Status)
        {
            case PulseSurveyAuditStatus.Draft:
                switch (next)
                {
                    case PulseSurveyAuditStatus.Pending:
                        return true;
                }
                return false;
            case PulseSurveyAuditStatus.Pending:
                return next == PulseSurveyAuditStatus.Active || next == PulseSurveyAuditStatus.Suspended;
            case PulseSurveyAuditStatus.Active:
                switch (next)
                {
                    case PulseSurveyAuditStatus.Suspended:
                    case PulseSurveyAuditStatus.Closed:
                        return true;
                }
                return false;
            case PulseSurveyAuditStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public PulseSurveyAudit Copy()
    {
        return new PulseSurveyAudit(Code, Label, JobCode, WindowDays, Category, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
