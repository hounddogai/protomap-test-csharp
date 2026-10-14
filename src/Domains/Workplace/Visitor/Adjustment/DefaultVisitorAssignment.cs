namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default visitor assignment is in its life cycle.</summary>
public enum DefaultVisitorAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A default visitor assignment. Its code is unique within a tenant.</summary>
public sealed class DefaultVisitorAssignment(string code, string label, string source, int tenureMonths, string description, bool active, DefaultVisitorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public bool Active { get; set; } = active;
    public DefaultVisitorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a default visitor assignment in status draft.</summary>
    public static DefaultVisitorAssignment NewDefaultVisitorAssignment(string code, string label)
    {
        return new DefaultVisitorAssignment(code, label, "", 0, "", false, DefaultVisitorAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultVisitorAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultVisitorAssignmentStatus next)
    {
        switch (Status)
        {
            case DefaultVisitorAssignmentStatus.Draft:
                return next == DefaultVisitorAssignmentStatus.Suspended;
            case DefaultVisitorAssignmentStatus.Pending:
                return next == DefaultVisitorAssignmentStatus.Active || next == DefaultVisitorAssignmentStatus.Closed;
            case DefaultVisitorAssignmentStatus.Active:
                switch (next)
                {
                    case DefaultVisitorAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case DefaultVisitorAssignmentStatus.Suspended:
                return next == DefaultVisitorAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(DefaultVisitorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default visitor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public DefaultVisitorAssignment Copy()
    {
        return new DefaultVisitorAssignment(Code, Label, Source, TenureMonths, Description, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
