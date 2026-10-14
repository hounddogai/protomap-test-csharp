namespace Acme.Workforce.Contractors.Offboarding.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding agreement is in its life cycle.</summary>
public enum OffboardingAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One offboarding agreement, with what the planners need to act on it.</summary>
public sealed class OffboardingAgreement(string code, string label, string name, string contactEmail, int leaveDays, OffboardingAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public int LeaveDays { get; set; } = leaveDays;
    public OffboardingAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding agreement in status open.</summary>
    public static OffboardingAgreement NewOffboardingAgreement(string code, string label)
    {
        return new OffboardingAgreement(code, label, "", "", 0, OffboardingAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingAgreementStatus next)
    {
        switch (Status)
        {
            case OffboardingAgreementStatus.Open:
                switch (next)
                {
                    case OffboardingAgreementStatus.Resolved:
                        return true;
                }
                return false;
            case OffboardingAgreementStatus.InReview:
                return next == OffboardingAgreementStatus.Reopened || next == OffboardingAgreementStatus.Archived;
            case OffboardingAgreementStatus.Resolved:
                switch (next)
                {
                    case OffboardingAgreementStatus.Archived:
                        return true;
                }
                return false;
            case OffboardingAgreementStatus.Reopened:
                return next == OffboardingAgreementStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OffboardingAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public OffboardingAgreement Copy()
    {
        return new OffboardingAgreement(Code, Label, Name, ContactEmail, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
