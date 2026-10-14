namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending pulse survey agreement is in its life cycle.</summary>
public enum PendingPulseSurveyAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pending pulse survey agreement, with what the planners need to act on it.</summary>
public sealed class PendingPulseSurveyAgreement(string code, string title, bool visible, string badge, string team, int createdAt, string locale, string employeeId, PendingPulseSurveyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public string Locale { get; set; } = locale;
    public string EmployeeId { get; set; } = employeeId;
    public PendingPulseSurveyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pending pulse survey agreement in status created.</summary>
    public static PendingPulseSurveyAgreement NewPendingPulseSurveyAgreement(string code, string title)
    {
        return new PendingPulseSurveyAgreement(
            code,
            title,
            false,
            "",
            "",
            0,
            "",
            "",
            PendingPulseSurveyAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingPulseSurveyAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingPulseSurveyAgreementStatus next)
    {
        switch (Status)
        {
            case PendingPulseSurveyAgreementStatus.Created:
                switch (next)
                {
                    case PendingPulseSurveyAgreementStatus.Validated:
                    case PendingPulseSurveyAgreementStatus.Published:
                        return true;
                }
                return false;
            case PendingPulseSurveyAgreementStatus.Validated:
                return next == PendingPulseSurveyAgreementStatus.Published;
            case PendingPulseSurveyAgreementStatus.Published:
                switch (next)
                {
                    case PendingPulseSurveyAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingPulseSurveyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending pulse survey agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public PendingPulseSurveyAgreement Copy()
    {
        return new PendingPulseSurveyAgreement(
            Code,
            Title,
            Visible,
            Badge,
            Team,
            CreatedAt,
            Locale,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
