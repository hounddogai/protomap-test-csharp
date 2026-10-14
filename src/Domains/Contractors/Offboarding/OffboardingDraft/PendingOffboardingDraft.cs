namespace Acme.Workforce.Contractors.Offboarding.OffboardingDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending offboarding draft is in its life cycle.</summary>
public enum PendingOffboardingDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending offboarding draft as the offboarding team keeps it.</summary>
public sealed class PendingOffboardingDraft(string code, string label, int tenureMonths, double score, double fte, string employeeId, int limit, PendingOffboardingDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Score { get; set; } = score;
    public double Fte { get; set; } = fte;
    public string EmployeeId { get; set; } = employeeId;
    public int Limit { get; set; } = limit;
    public PendingOffboardingDraftStatus Status { get; set; } = status;

    /// <summary>Starts a pending offboarding draft in status requested.</summary>
    public static PendingOffboardingDraft NewPendingOffboardingDraft(string code, string label)
    {
        return new PendingOffboardingDraft(code, label, 0, 0.0, 0.0, "", 0, PendingOffboardingDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingOffboardingDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingOffboardingDraftStatus next)
    {
        switch (Status)
        {
            case PendingOffboardingDraftStatus.Requested:
                return next == PendingOffboardingDraftStatus.Revoked;
            case PendingOffboardingDraftStatus.Granted:
                switch (next)
                {
                    case PendingOffboardingDraftStatus.Expired:
                        return true;
                }
                return false;
            case PendingOffboardingDraftStatus.Revoked:
                return next == PendingOffboardingDraftStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingOffboardingDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending offboarding draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit + Score, 2);
    }

    public PendingOffboardingDraft Copy()
    {
        return new PendingOffboardingDraft(Code, Label, TenureMonths, Score, Fte, EmployeeId, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
