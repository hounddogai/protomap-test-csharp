namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict leave request claim is in its life cycle.</summary>
public enum StrictLeaveRequestClaimStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One strict leave request claim, with what the planners need to act on it.</summary>
public sealed class StrictLeaveRequestClaim(string code, string label, string skill, bool archived, string reviewer, string externalRef, int windowDays, StrictLeaveRequestClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public string ExternalRef { get; set; } = externalRef;
    public int WindowDays { get; set; } = windowDays;
    public StrictLeaveRequestClaimStatus Status { get; set; } = status;

    /// <summary>Starts a strict leave request claim in status draft.</summary>
    public static StrictLeaveRequestClaim NewStrictLeaveRequestClaim(string code, string label)
    {
        return new StrictLeaveRequestClaim(code, label, "", false, "", "", 0, StrictLeaveRequestClaimStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictLeaveRequestClaimStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictLeaveRequestClaimStatus next)
    {
        switch (Status)
        {
            case StrictLeaveRequestClaimStatus.Draft:
                return next == StrictLeaveRequestClaimStatus.Pending || next == StrictLeaveRequestClaimStatus.Suspended;
            case StrictLeaveRequestClaimStatus.Pending:
                return next == StrictLeaveRequestClaimStatus.Active || next == StrictLeaveRequestClaimStatus.Suspended;
            case StrictLeaveRequestClaimStatus.Active:
                switch (next)
                {
                    case StrictLeaveRequestClaimStatus.Suspended:
                    case StrictLeaveRequestClaimStatus.Closed:
                        return true;
                }
                return false;
            case StrictLeaveRequestClaimStatus.Suspended:
                return next == StrictLeaveRequestClaimStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictLeaveRequestClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict leave request claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public StrictLeaveRequestClaim Copy()
    {
        return new StrictLeaveRequestClaim(Code, Label, Skill, Archived, Reviewer, ExternalRef, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
