namespace Acme.Workforce.Absence.LeaveRequest.RequestClaim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient leave request claim is in its life cycle.</summary>
public enum LenientLeaveRequestClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One lenient leave request claim, with what the planners need to act on it.</summary>
public sealed class LenientLeaveRequestClaim(string code, string label, string employeeId, string reviewer, double amount, string phone, int tenureMonths, int version, int expiresAt, string name, LenientLeaveRequestClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public string Reviewer { get; set; } = reviewer;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Version { get; set; } = version;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Name { get; set; } = name;
    public LenientLeaveRequestClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient leave request claim in status open.</summary>
    public static LenientLeaveRequestClaim NewLenientLeaveRequestClaim(string code, string label)
    {
        return new LenientLeaveRequestClaim(
            code,
            label,
            "",
            "",
            0.0,
            "",
            0,
            0,
            0,
            "",
            LenientLeaveRequestClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLeaveRequestClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLeaveRequestClaimStatus next)
    {
        switch (Status)
        {
            case LenientLeaveRequestClaimStatus.Open:
                switch (next)
                {
                    case LenientLeaveRequestClaimStatus.InReview:
                        return true;
                }
                return false;
            case LenientLeaveRequestClaimStatus.InReview:
                return next == LenientLeaveRequestClaimStatus.Reopened || next == LenientLeaveRequestClaimStatus.Archived;
            case LenientLeaveRequestClaimStatus.Resolved:
                switch (next)
                {
                    case LenientLeaveRequestClaimStatus.Reopened:
                    case LenientLeaveRequestClaimStatus.Archived:
                        return true;
                }
                return false;
            case LenientLeaveRequestClaimStatus.Reopened:
                switch (next)
                {
                    case LenientLeaveRequestClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLeaveRequestClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient leave request claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)ExpiresAt * (double)Version, 2);
    }

    public LenientLeaveRequestClaim Copy()
    {
        return new LenientLeaveRequestClaim(
            Code,
            Label,
            EmployeeId,
            Reviewer,
            Amount,
            Phone,
            TenureMonths,
            Version,
            ExpiresAt,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
