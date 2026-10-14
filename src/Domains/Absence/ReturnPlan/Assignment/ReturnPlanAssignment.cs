namespace Acme.Workforce.Absence.ReturnPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan assignment is in its life cycle.</summary>
public enum ReturnPlanAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan assignment. Its code is unique within a tenant.</summary>
public sealed class ReturnPlanAssignment(string code, string name, double fte, string salaryBand, string summary, ReturnPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string SalaryBand { get; set; } = salaryBand;
    public string Summary { get; set; } = summary;
    public ReturnPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a return plan assignment in status proposed.</summary>
    public static ReturnPlanAssignment NewReturnPlanAssignment(string code, string name)
    {
        return new ReturnPlanAssignment(code, name, 0.0, "", "", ReturnPlanAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case ReturnPlanAssignmentStatus.Proposed:
                switch (next)
                {
                    case ReturnPlanAssignmentStatus.Rejected:
                        return true;
                }
                return false;
            case ReturnPlanAssignmentStatus.Approved:
                return next == ReturnPlanAssignmentStatus.Withdrawn;
            case ReturnPlanAssignmentStatus.Rejected:
                switch (next)
                {
                    case ReturnPlanAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReturnPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public ReturnPlanAssignment Copy()
    {
        return new ReturnPlanAssignment(Code, Name, Fte, SalaryBand, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
