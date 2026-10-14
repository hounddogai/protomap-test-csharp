namespace Acme.Workforce.Engagement.PulseSurvey.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pulse survey audit is in its life cycle.</summary>
public enum PendingPulseSurveyAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending pulse survey audit, with what the planners need to act on it.</summary>
public sealed class PendingPulseSurveyAudit(string code, string title, string locale, int leaveDays, string region, PendingPulseSurveyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public string Region { get; set; } = region;
    public PendingPulseSurveyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a pending pulse survey audit in status idle.</summary>
    public static PendingPulseSurveyAudit NewPendingPulseSurveyAudit(string code, string title)
    {
        return new PendingPulseSurveyAudit(code, title, "", 0, "", PendingPulseSurveyAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPulseSurveyAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPulseSurveyAuditStatus next)
    {
        switch (Status)
        {
            case PendingPulseSurveyAuditStatus.Idle:
                return next == PendingPulseSurveyAuditStatus.Warming || next == PendingPulseSurveyAuditStatus.Stopped;
            case PendingPulseSurveyAuditStatus.Warming:
                switch (next)
                {
                    case PendingPulseSurveyAuditStatus.Ready:
                    case PendingPulseSurveyAuditStatus.Stopped:
                        return true;
                }
                return false;
            case PendingPulseSurveyAuditStatus.Ready:
                switch (next)
                {
                    case PendingPulseSurveyAuditStatus.Stopped:
                        return true;
                }
                return false;
            case PendingPulseSurveyAuditStatus.Draining:
                switch (next)
                {
                    case PendingPulseSurveyAuditStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPulseSurveyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pulse survey audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public PendingPulseSurveyAudit Copy()
    {
        return new PendingPulseSurveyAudit(Code, Title, Locale, LeaveDays, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
