namespace Acme.Workforce.Engagement.PulseSurvey.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal pulse survey audit is in its life cycle.</summary>
public enum SeasonalPulseSurveyAuditStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a seasonal pulse survey audit and its current status.</summary>
public sealed class SeasonalPulseSurveyAudit(string code, string label, int leaveDays, bool visible, string checksum, SeasonalPulseSurveyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Visible { get; set; } = visible;
    public string Checksum { get; set; } = checksum;
    public SeasonalPulseSurveyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal pulse survey audit in status requested.</summary>
    public static SeasonalPulseSurveyAudit NewSeasonalPulseSurveyAudit(string code, string label)
    {
        return new SeasonalPulseSurveyAudit(code, label, 0, false, "", SeasonalPulseSurveyAuditStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalPulseSurveyAuditStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalPulseSurveyAuditStatus next)
    {
        switch (Status)
        {
            case SeasonalPulseSurveyAuditStatus.Requested:
                return next == SeasonalPulseSurveyAuditStatus.Revoked;
            case SeasonalPulseSurveyAuditStatus.Granted:
                return next == SeasonalPulseSurveyAuditStatus.Expired;
            case SeasonalPulseSurveyAuditStatus.Revoked:
                switch (next)
                {
                    case SeasonalPulseSurveyAuditStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalPulseSurveyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal pulse survey audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public SeasonalPulseSurveyAudit Copy()
    {
        return new SeasonalPulseSurveyAudit(Code, Label, LeaveDays, Visible, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
