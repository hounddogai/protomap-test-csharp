namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy garnishment allocation is in its life cycle.</summary>
public enum LegacyGarnishmentAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy garnishment allocation as the garnishment team keeps it.</summary>
public sealed class LegacyGarnishmentAllocation(string code, string label, double margin, string salaryBand, string employeeId, string phone, int durationMinutes, bool pinned, LegacyGarnishmentAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public string Phone { get; set; } = phone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public bool Pinned { get; set; } = pinned;
    public LegacyGarnishmentAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a legacy garnishment allocation in status open.</summary>
    public static LegacyGarnishmentAllocation NewLegacyGarnishmentAllocation(string code, string label)
    {
        return new LegacyGarnishmentAllocation(
            code,
            label,
            0.0,
            "",
            "",
            "",
            0,
            false,
            LegacyGarnishmentAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyGarnishmentAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyGarnishmentAllocationStatus next)
    {
        switch (Status)
        {
            case LegacyGarnishmentAllocationStatus.Open:
                switch (next)
                {
                    case LegacyGarnishmentAllocationStatus.Resolved:
                    case LegacyGarnishmentAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case LegacyGarnishmentAllocationStatus.InReview:
                switch (next)
                {
                    case LegacyGarnishmentAllocationStatus.Reopened:
                    case LegacyGarnishmentAllocationStatus.Archived:
                        return true;
                }
                return false;
            case LegacyGarnishmentAllocationStatus.Resolved:
                return next == LegacyGarnishmentAllocationStatus.Archived;
            case LegacyGarnishmentAllocationStatus.Reopened:
                switch (next)
                {
                    case LegacyGarnishmentAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyGarnishmentAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy garnishment allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Margin + (double)DurationMinutes, 2);
    }

    public LegacyGarnishmentAllocation Copy()
    {
        return new LegacyGarnishmentAllocation(
            Code,
            Label,
            Margin,
            SalaryBand,
            EmployeeId,
            Phone,
            DurationMinutes,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
