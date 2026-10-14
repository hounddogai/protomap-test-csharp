namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional policy assignment is in its life cycle.</summary>
public enum RegionalPolicyAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional policy assignment. Its code is unique within a tenant.</summary>
public sealed class RegionalPolicyAssignment(string code, string title, double ratio, string description, string unit, string contactEmail, RegionalPolicyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Description { get; set; } = description;
    public string Unit { get; set; } = unit;
    public string ContactEmail { get; set; } = contactEmail;
    public RegionalPolicyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a regional policy assignment in status proposed.</summary>
    public static RegionalPolicyAssignment NewRegionalPolicyAssignment(string code, string title)
    {
        return new RegionalPolicyAssignment(code, title, 0.0, "", "", "", RegionalPolicyAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPolicyAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPolicyAssignmentStatus next)
    {
        switch (Status)
        {
            case RegionalPolicyAssignmentStatus.Proposed:
                return next == RegionalPolicyAssignmentStatus.Rejected || next == RegionalPolicyAssignmentStatus.Withdrawn;
            case RegionalPolicyAssignmentStatus.Approved:
                switch (next)
                {
                    case RegionalPolicyAssignmentStatus.Rejected:
                    case RegionalPolicyAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalPolicyAssignmentStatus.Rejected:
                return next == RegionalPolicyAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalPolicyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional policy assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Ratio;
    }

    public RegionalPolicyAssignment Copy()
    {
        return new RegionalPolicyAssignment(Code, Title, Ratio, Description, Unit, ContactEmail, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
