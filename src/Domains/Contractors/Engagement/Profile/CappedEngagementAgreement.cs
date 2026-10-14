namespace Acme.Workforce.Contractors.Engagement.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped engagement agreement is in its life cycle.</summary>
public enum CappedEngagementAgreementStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A capped engagement agreement. Its code is unique within a tenant.</summary>
public sealed class CappedEngagementAgreement(string code, string name, int leaveDays, string contactEmail, string reviewer, int graceDays, double hours, int windowDays, bool archived, CappedEngagementAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public string ContactEmail { get; set; } = contactEmail;
    public string Reviewer { get; set; } = reviewer;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public int WindowDays { get; set; } = windowDays;
    public bool Archived { get; set; } = archived;
    public CappedEngagementAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a capped engagement agreement in status new.</summary>
    public static CappedEngagementAgreement NewCappedEngagementAgreement(string code, string name)
    {
        return new CappedEngagementAgreement(
            code,
            name,
            0,
            "",
            "",
            0,
            0.0,
            0,
            false,
            CappedEngagementAgreementStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedEngagementAgreementStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedEngagementAgreementStatus next)
    {
        switch (Status)
        {
            case CappedEngagementAgreementStatus.New:
                return next == CappedEngagementAgreementStatus.Failed;
            case CappedEngagementAgreementStatus.Queued:
                return next == CappedEngagementAgreementStatus.Failed;
            case CappedEngagementAgreementStatus.Running:
                return next == CappedEngagementAgreementStatus.Done || next == CappedEngagementAgreementStatus.Failed;
            case CappedEngagementAgreementStatus.Done:
                switch (next)
                {
                    case CappedEngagementAgreementStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedEngagementAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped engagement agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Hours + (double)LeaveDays, 2);
    }

    public CappedEngagementAgreement Copy()
    {
        return new CappedEngagementAgreement(
            Code,
            Name,
            LeaveDays,
            ContactEmail,
            Reviewer,
            GraceDays,
            Hours,
            WindowDays,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
