namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default retro pay allocation is in its life cycle.</summary>
public enum DefaultRetroPayAllocationStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default retro pay allocation. Its code is unique within a tenant.</summary>
public sealed class DefaultRetroPayAllocation(string code, string title, double discount, string department, int tenureMonths, string grade, bool visible, DefaultRetroPayAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Grade { get; set; } = grade;
    public bool Visible { get; set; } = visible;
    public DefaultRetroPayAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a default retro pay allocation in status proposed.</summary>
    public static DefaultRetroPayAllocation NewDefaultRetroPayAllocation(string code, string title)
    {
        return new DefaultRetroPayAllocation(
            code,
            title,
            0.0,
            "",
            0,
            "",
            false,
            DefaultRetroPayAllocationStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultRetroPayAllocationStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultRetroPayAllocationStatus next)
    {
        switch (Status)
        {
            case DefaultRetroPayAllocationStatus.Proposed:
                return next == DefaultRetroPayAllocationStatus.Withdrawn;
            case DefaultRetroPayAllocationStatus.Approved:
                return next == DefaultRetroPayAllocationStatus.Rejected;
            case DefaultRetroPayAllocationStatus.Rejected:
                switch (next)
                {
                    case DefaultRetroPayAllocationStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultRetroPayAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default retro pay allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round(Discount * (double)TenureMonths, 2);
    }

    public DefaultRetroPayAllocation Copy()
    {
        return new DefaultRetroPayAllocation(
            Code,
            Title,
            Discount,
            Department,
            TenureMonths,
            Grade,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
