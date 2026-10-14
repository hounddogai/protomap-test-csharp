namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient desk allocation is in its life cycle.</summary>
public enum LenientDeskAllocationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One lenient desk allocation, with what the planners need to act on it.</summary>
public sealed class LenientDeskAllocation(string code, string title, string location, double rate, string currency, string skill, string summary, string employeeId, string jobCode, LenientDeskAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Location { get; set; } = location;
    public double Rate { get; set; } = rate;
    public string Currency { get; set; } = currency;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public string JobCode { get; set; } = jobCode;
    public LenientDeskAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a lenient desk allocation in status requested.</summary>
    public static LenientDeskAllocation NewLenientDeskAllocation(string code, string title)
    {
        return new LenientDeskAllocation(
            code,
            title,
            "",
            0.0,
            "",
            "",
            "",
            "",
            "",
            LenientDeskAllocationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientDeskAllocationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientDeskAllocationStatus next)
    {
        switch (Status)
        {
            case LenientDeskAllocationStatus.Requested:
                switch (next)
                {
                    case LenientDeskAllocationStatus.Granted:
                    case LenientDeskAllocationStatus.Revoked:
                        return true;
                }
                return false;
            case LenientDeskAllocationStatus.Granted:
                return next == LenientDeskAllocationStatus.Expired;
            case LenientDeskAllocationStatus.Revoked:
                switch (next)
                {
                    case LenientDeskAllocationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientDeskAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient desk allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public LenientDeskAllocation Copy()
    {
        return new LenientDeskAllocation(
            Code,
            Title,
            Location,
            Rate,
            Currency,
            Skill,
            Summary,
            EmployeeId,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
