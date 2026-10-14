namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient bonus pool claim is in its life cycle.</summary>
public enum LenientBonusPoolClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient bonus pool claim and its current status.</summary>
public sealed class LenientBonusPoolClaim(string code, string name, string jobCode, string location, string reviewer, string segment, string employeeId, int tenureMonths, LenientBonusPoolClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public string Reviewer { get; set; } = reviewer;
    public string Segment { get; set; } = segment;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public LenientBonusPoolClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient bonus pool claim in status planned.</summary>
    public static LenientBonusPoolClaim NewLenientBonusPoolClaim(string code, string name)
    {
        return new LenientBonusPoolClaim(code, name, "", "", "", "", "", 0, LenientBonusPoolClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientBonusPoolClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientBonusPoolClaimStatus next)
    {
        switch (Status)
        {
            case LenientBonusPoolClaimStatus.Planned:
                switch (next)
                {
                    case LenientBonusPoolClaimStatus.InProgress:
                    case LenientBonusPoolClaimStatus.Completed:
                        return true;
                }
                return false;
            case LenientBonusPoolClaimStatus.Scheduled:
                return next == LenientBonusPoolClaimStatus.InProgress || next == LenientBonusPoolClaimStatus.Completed;
            case LenientBonusPoolClaimStatus.InProgress:
                switch (next)
                {
                    case LenientBonusPoolClaimStatus.Completed:
                    case LenientBonusPoolClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientBonusPoolClaimStatus.Completed:
                return next == LenientBonusPoolClaimStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientBonusPoolClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient bonus pool claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient bonus pool claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public LenientBonusPoolClaim Copy()
    {
        return new LenientBonusPoolClaim(
            Code,
            Name,
            JobCode,
            Location,
            Reviewer,
            Segment,
            EmployeeId,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
