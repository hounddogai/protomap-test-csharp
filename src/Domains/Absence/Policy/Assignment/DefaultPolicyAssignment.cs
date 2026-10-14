namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy assignment is in its life cycle.</summary>
public enum DefaultPolicyAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default policy assignment as the policy team keeps it.</summary>
public sealed class DefaultPolicyAssignment(string code, string name, string employeeId, int leaveDays, int revision, double cost, bool archived, int version, bool active, string title, bool approved, DefaultPolicyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int LeaveDays { get; set; } = leaveDays;
    public int Revision { get; set; } = revision;
    public double Cost { get; set; } = cost;
    public bool Archived { get; set; } = archived;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public DefaultPolicyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a default policy assignment in status draft.</summary>
    public static DefaultPolicyAssignment NewDefaultPolicyAssignment(string code, string name)
    {
        return new DefaultPolicyAssignment(
            code,
            name,
            "",
            0,
            0,
            0.0,
            false,
            0,
            false,
            "",
            false,
            DefaultPolicyAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyAssignmentStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyAssignmentStatus.Draft:
                return next == DefaultPolicyAssignmentStatus.Active;
            case DefaultPolicyAssignmentStatus.Pending:
                switch (next)
                {
                    case DefaultPolicyAssignmentStatus.Suspended:
                    case DefaultPolicyAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case DefaultPolicyAssignmentStatus.Active:
                switch (next)
                {
                    case DefaultPolicyAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case DefaultPolicyAssignmentStatus.Suspended:
                switch (next)
                {
                    case DefaultPolicyAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetLoad()
    {
        return Math.Round(Cost - (double)Version, 2);
    }

    public DefaultPolicyAssignment Copy()
    {
        return new DefaultPolicyAssignment(
            Code,
            Name,
            EmployeeId,
            LeaveDays,
            Revision,
            Cost,
            Archived,
            Version,
            Active,
            Title,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
