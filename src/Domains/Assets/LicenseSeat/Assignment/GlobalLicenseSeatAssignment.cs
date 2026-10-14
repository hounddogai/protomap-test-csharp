namespace Acme.Workforce.Assets.LicenseSeat.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global license seat assignment is in its life cycle.</summary>
public enum GlobalLicenseSeatAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A global license seat assignment. Its code is unique within a tenant.</summary>
public sealed class GlobalLicenseSeatAssignment(string code, string name, double minimum, string employeeId, string department, GlobalLicenseSeatAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public string Department { get; set; } = department;
    public GlobalLicenseSeatAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a global license seat assignment in status open.</summary>
    public static GlobalLicenseSeatAssignment NewGlobalLicenseSeatAssignment(string code, string name)
    {
        return new GlobalLicenseSeatAssignment(code, name, 0.0, "", "", GlobalLicenseSeatAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLicenseSeatAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLicenseSeatAssignmentStatus next)
    {
        switch (Status)
        {
            case GlobalLicenseSeatAssignmentStatus.Open:
                return next == GlobalLicenseSeatAssignmentStatus.Resolved || next == GlobalLicenseSeatAssignmentStatus.Reopened;
            case GlobalLicenseSeatAssignmentStatus.InReview:
                switch (next)
                {
                    case GlobalLicenseSeatAssignmentStatus.Reopened:
                        return true;
                }
                return false;
            case GlobalLicenseSeatAssignmentStatus.Resolved:
                return next == GlobalLicenseSeatAssignmentStatus.Reopened || next == GlobalLicenseSeatAssignmentStatus.Archived;
            case GlobalLicenseSeatAssignmentStatus.Reopened:
                return next == GlobalLicenseSeatAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GlobalLicenseSeatAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global license seat assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"global license seat assignment"} {Code}";
    }

    public double MeasuredValue()
    {
        return Minimum;
    }

    public GlobalLicenseSeatAssignment Copy()
    {
        return new GlobalLicenseSeatAssignment(Code, Name, Minimum, EmployeeId, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
