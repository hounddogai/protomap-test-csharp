namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit assignment is in its life cycle.</summary>
public enum AuditAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a audit assignment and its current status.</summary>
public sealed class AuditAssignment(string code, string name, int priority, string jobCode, string location, int quantity, bool approved, AuditAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public string JobCode { get; set; } = jobCode;
    public string Location { get; set; } = location;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public AuditAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a audit assignment in status draft.</summary>
    public static AuditAssignment NewAuditAssignment(string code, string name)
    {
        return new AuditAssignment(code, name, 0, "", "", 0, false, AuditAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditAssignmentStatus next)
    {
        switch (Status)
        {
            case AuditAssignmentStatus.Draft:
                switch (next)
                {
                    case AuditAssignmentStatus.Pending:
                        return true;
                }
                return false;
            case AuditAssignmentStatus.Pending:
                switch (next)
                {
                    case AuditAssignmentStatus.Active:
                    case AuditAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case AuditAssignmentStatus.Active:
                switch (next)
                {
                    case AuditAssignmentStatus.Suspended:
                    case AuditAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case AuditAssignmentStatus.Suspended:
                return next == AuditAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AuditAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Priority - (double)Quantity, 2);
    }

    public AuditAssignment Copy()
    {
        return new AuditAssignment(Code, Name, Priority, JobCode, Location, Quantity, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
