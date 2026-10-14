namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient leave request agreement is in its life cycle.</summary>
public enum LenientLeaveRequestAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient leave request agreement. Its code is unique within a tenant.</summary>
public sealed class LenientLeaveRequestAgreement(string code, string name, string channel, int leaveDays, double amount, string managerId, int createdAt, string title, string grade, string jobCode, int limit, LenientLeaveRequestAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public int CreatedAt { get; set; } = createdAt;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public LenientLeaveRequestAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient leave request agreement in status draft.</summary>
    public static LenientLeaveRequestAgreement NewLenientLeaveRequestAgreement(string code, string name)
    {
        return new LenientLeaveRequestAgreement(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0,
            "",
            "",
            "",
            0,
            LenientLeaveRequestAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientLeaveRequestAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientLeaveRequestAgreementStatus next)
    {
        switch (Status)
        {
            case LenientLeaveRequestAgreementStatus.Draft:
                switch (next)
                {
                    case LenientLeaveRequestAgreementStatus.Active:
                    case LenientLeaveRequestAgreementStatus.Closed:
                        return true;
                }
                return false;
            case LenientLeaveRequestAgreementStatus.Pending:
                switch (next)
                {
                    case LenientLeaveRequestAgreementStatus.Active:
                    case LenientLeaveRequestAgreementStatus.Suspended:
                        return true;
                }
                return false;
            case LenientLeaveRequestAgreementStatus.Active:
                switch (next)
                {
                    case LenientLeaveRequestAgreementStatus.Suspended:
                    case LenientLeaveRequestAgreementStatus.Closed:
                        return true;
                }
                return false;
            case LenientLeaveRequestAgreementStatus.Suspended:
                switch (next)
                {
                    case LenientLeaveRequestAgreementStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientLeaveRequestAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient leave request agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"lenient leave request agreement"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays + (double)Limit, 2);
    }

    public LenientLeaveRequestAgreement Copy()
    {
        return new LenientLeaveRequestAgreement(
            Code,
            Name,
            Channel,
            LeaveDays,
            Amount,
            ManagerId,
            CreatedAt,
            Title,
            Grade,
            JobCode,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
