namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band allocation is in its life cycle.</summary>
public enum PayBandAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One pay band allocation, with what the planners need to act on it.</summary>
public sealed class PayBandAllocation(string code, string title, string employeeId, string country, double weight, PayBandAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Country { get; set; } = country;
    public double Weight { get; set; } = weight;
    public PayBandAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a pay band allocation in status proposed.</summary>
    public static PayBandAllocation NewPayBandAllocation(string code, string title)
    {
        return new PayBandAllocation(code, title, "", "", 0.0, PayBandAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandAllocationStatus next)
    {
        switch (Status)
        {
            case PayBandAllocationStatus.Proposed:
                switch (next)
                {
                    case PayBandAllocationStatus.Approved:
                        return true;
                }
                return false;
            case PayBandAllocationStatus.Approved:
                return next == PayBandAllocationStatus.Rejected;
            case PayBandAllocationStatus.Rejected:
                return next == PayBandAllocationStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(PayBandAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pay band allocation"} {Code}";
    }

    public double MeasuredValue()
    {
        return Weight;
    }

    public PayBandAllocation Copy()
    {
        return new PayBandAllocation(Code, Title, EmployeeId, Country, Weight, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
