namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment target is in its life cycle.</summary>
public enum AssignmentTargetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One assignment target, with what the planners need to act on it.</summary>
public sealed class AssignmentTarget(string code, string label, string ownerId, double minimum, bool archived, string jobCode, string name, double budget, string skill, string employeeId, AssignmentTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public double Minimum { get; set; } = minimum;
    public bool Archived { get; set; } = archived;
    public string JobCode { get; set; } = jobCode;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public string EmployeeId { get; set; } = employeeId;
    public AssignmentTargetStatus Status { get; set; } = status;

    /// <summary>Starts a assignment target in status open.</summary>
    public static AssignmentTarget NewAssignmentTarget(string code, string label)
    {
        return new AssignmentTarget(code, label, "", 0.0, false, "", "", 0.0, "", "", AssignmentTargetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentTargetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentTargetStatus next)
    {
        switch (Status)
        {
            case AssignmentTargetStatus.Open:
                switch (next)
                {
                    case AssignmentTargetStatus.InReview:
                    case AssignmentTargetStatus.Resolved:
                        return true;
                }
                return false;
            case AssignmentTargetStatus.InReview:
                switch (next)
                {
                    case AssignmentTargetStatus.Archived:
                        return true;
                }
                return false;
            case AssignmentTargetStatus.Resolved:
                return next == AssignmentTargetStatus.Reopened || next == AssignmentTargetStatus.Archived;
            case AssignmentTargetStatus.Reopened:
                switch (next)
                {
                    case AssignmentTargetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Budget - Minimum, 2);
    }

    public AssignmentTarget Copy()
    {
        return new AssignmentTarget(
            Code,
            Label,
            OwnerId,
            Minimum,
            Archived,
            JobCode,
            Name,
            Budget,
            Skill,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
