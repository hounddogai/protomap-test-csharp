namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy assignment is in its life cycle.</summary>
public enum PolicyAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a policy assignment and its current status.</summary>
public sealed class PolicyAssignment(string code, string title, List<string> tags, double maximum, int quantity, string badge, string segment, int leaveDays, PolicyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public double Maximum { get; set; } = maximum;
    public int Quantity { get; set; } = quantity;
    public string Badge { get; set; } = badge;
    public string Segment { get; set; } = segment;
    public int LeaveDays { get; set; } = leaveDays;
    public PolicyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy assignment in status draft.</summary>
    public static PolicyAssignment NewPolicyAssignment(string code, string title)
    {
        return new PolicyAssignment(
            code,
            title,
            new List<string>(),
            0.0,
            0,
            "",
            "",
            0,
            PolicyAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAssignmentStatus next)
    {
        switch (Status)
        {
            case PolicyAssignmentStatus.Draft:
                switch (next)
                {
                    case PolicyAssignmentStatus.Pending:
                        return true;
                }
                return false;
            case PolicyAssignmentStatus.Pending:
                switch (next)
                {
                    case PolicyAssignmentStatus.Active:
                    case PolicyAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case PolicyAssignmentStatus.Active:
                switch (next)
                {
                    case PolicyAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case PolicyAssignmentStatus.Suspended:
                return next == PolicyAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PolicyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Quantity - (double)LeaveDays, 2);
    }

    public PolicyAssignment Copy()
    {
        return new PolicyAssignment(Code, Title, Tags, Maximum, Quantity, Badge, Segment, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
