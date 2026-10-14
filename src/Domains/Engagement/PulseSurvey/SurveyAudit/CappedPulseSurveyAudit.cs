namespace Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped pulse survey audit is in its life cycle.</summary>
public enum CappedPulseSurveyAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A capped pulse survey audit. Its code is unique within a tenant.</summary>
public sealed class CappedPulseSurveyAudit(string code, string name, string locale, string timezone, string externalRef, double minimum, int quantity, CappedPulseSurveyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public string Timezone { get; set; } = timezone;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public CappedPulseSurveyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a capped pulse survey audit in status open.</summary>
    public static CappedPulseSurveyAudit NewCappedPulseSurveyAudit(string code, string name)
    {
        return new CappedPulseSurveyAudit(code, name, "", "", "", 0.0, 0, CappedPulseSurveyAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedPulseSurveyAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedPulseSurveyAuditStatus next)
    {
        switch (Status)
        {
            case CappedPulseSurveyAuditStatus.Open:
                return next == CappedPulseSurveyAuditStatus.InReview || next == CappedPulseSurveyAuditStatus.Archived;
            case CappedPulseSurveyAuditStatus.InReview:
                switch (next)
                {
                    case CappedPulseSurveyAuditStatus.Archived:
                        return true;
                }
                return false;
            case CappedPulseSurveyAuditStatus.Resolved:
                switch (next)
                {
                    case CappedPulseSurveyAuditStatus.Reopened:
                        return true;
                }
                return false;
            case CappedPulseSurveyAuditStatus.Reopened:
                switch (next)
                {
                    case CappedPulseSurveyAuditStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedPulseSurveyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped pulse survey audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Quantity - Minimum, 2);
    }

    public CappedPulseSurveyAudit Copy()
    {
        return new CappedPulseSurveyAudit(Code, Name, Locale, Timezone, ExternalRef, Minimum, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
