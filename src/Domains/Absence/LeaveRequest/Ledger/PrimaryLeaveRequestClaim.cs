namespace Acme.Workforce.Absence.LeaveRequest.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary leave request claim is in its life cycle.</summary>
public enum PrimaryLeaveRequestClaimStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A primary leave request claim as the leave request team keeps it.</summary>
public sealed class PrimaryLeaveRequestClaim(string code, string name, bool approved, int leaveDays, string skill, int batchSize, double floorValue, PrimaryLeaveRequestClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Approved { get; set; } = approved;
    public int LeaveDays { get; set; } = leaveDays;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public double FloorValue { get; set; } = floorValue;
    public PrimaryLeaveRequestClaimStatus Status { get; set; } = status;

    /// <summary>Starts a primary leave request claim in status requested.</summary>
    public static PrimaryLeaveRequestClaim NewPrimaryLeaveRequestClaim(string code, string name)
    {
        return new PrimaryLeaveRequestClaim(
            code,
            name,
            false,
            0,
            "",
            0,
            0.0,
            PrimaryLeaveRequestClaimStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLeaveRequestClaimStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLeaveRequestClaimStatus next)
    {
        switch (Status)
        {
            case PrimaryLeaveRequestClaimStatus.Requested:
                return next == PrimaryLeaveRequestClaimStatus.Granted;
            case PrimaryLeaveRequestClaimStatus.Granted:
                return next == PrimaryLeaveRequestClaimStatus.Expired;
            case PrimaryLeaveRequestClaimStatus.Revoked:
                switch (next)
                {
                    case PrimaryLeaveRequestClaimStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryLeaveRequestClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary leave request claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary leave request claim"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)BatchSize * (double)LeaveDays, 2);
    }

    public PrimaryLeaveRequestClaim Copy()
    {
        return new PrimaryLeaveRequestClaim(Code, Name, Approved, LeaveDays, Skill, BatchSize, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
