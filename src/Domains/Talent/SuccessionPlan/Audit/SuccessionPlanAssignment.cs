namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan assignment is in its life cycle.</summary>
public enum SuccessionPlanAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One succession plan assignment, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanAssignment(string code, string label, double budget, string location, int leaveDays, double floorValue, int attempts, string summary, string contactEmail, string externalRef, string timezone, SuccessionPlanAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public string Summary { get; set; } = summary;
    public string ContactEmail { get; set; } = contactEmail;
    public string ExternalRef { get; set; } = externalRef;
    public string Timezone { get; set; } = timezone;
    public SuccessionPlanAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan assignment in status draft.</summary>
    public static SuccessionPlanAssignment NewSuccessionPlanAssignment(string code, string label)
    {
        return new SuccessionPlanAssignment(
            code,
            label,
            0.0,
            "",
            0,
            0.0,
            0,
            "",
            "",
            "",
            "",
            SuccessionPlanAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanAssignmentStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanAssignmentStatus.Draft:
                switch (next)
                {
                    case SuccessionPlanAssignmentStatus.Active:
                        return true;
                }
                return false;
            case SuccessionPlanAssignmentStatus.Pending:
                return next == SuccessionPlanAssignmentStatus.Active;
            case SuccessionPlanAssignmentStatus.Active:
                switch (next)
                {
                    case SuccessionPlanAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case SuccessionPlanAssignmentStatus.Suspended:
                switch (next)
                {
                    case SuccessionPlanAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"succession plan assignment"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Budget - (double)LeaveDays, 2);
    }

    public SuccessionPlanAssignment Copy()
    {
        return new SuccessionPlanAssignment(
            Code,
            Label,
            Budget,
            Location,
            LeaveDays,
            FloorValue,
            Attempts,
            Summary,
            ContactEmail,
            ExternalRef,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
