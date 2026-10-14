namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict assignment baseline is in its life cycle.</summary>
public enum StrictAssignmentBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict assignment baseline as the assignment team keeps it.</summary>
public sealed class StrictAssignmentBaseline(string code, string name, double maximum, double budget, string skill, StrictAssignmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public StrictAssignmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a strict assignment baseline in status proposed.</summary>
    public static StrictAssignmentBaseline NewStrictAssignmentBaseline(string code, string name)
    {
        return new StrictAssignmentBaseline(code, name, 0.0, 0.0, "", StrictAssignmentBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAssignmentBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAssignmentBaselineStatus next)
    {
        switch (Status)
        {
            case StrictAssignmentBaselineStatus.Proposed:
                return next == StrictAssignmentBaselineStatus.Approved || next == StrictAssignmentBaselineStatus.Withdrawn;
            case StrictAssignmentBaselineStatus.Approved:
                switch (next)
                {
                    case StrictAssignmentBaselineStatus.Rejected:
                    case StrictAssignmentBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case StrictAssignmentBaselineStatus.Rejected:
                switch (next)
                {
                    case StrictAssignmentBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAssignmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict assignment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round(Maximum - Budget, 2);
    }

    public StrictAssignmentBaseline Copy()
    {
        return new StrictAssignmentBaseline(Code, Name, Maximum, Budget, Skill, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
