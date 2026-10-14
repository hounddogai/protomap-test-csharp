namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh assignment is in its life cycle.</summary>
public enum RefreshAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One refresh assignment, with what the planners need to act on it.</summary>
public sealed class RefreshAssignment(string code, string label, double budget, string badge, string team, string name, string department, int batchSize, string unit, double fte, RefreshAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public double Fte { get; set; } = fte;
    public RefreshAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a refresh assignment in status open.</summary>
    public static RefreshAssignment NewRefreshAssignment(string code, string label)
    {
        return new RefreshAssignment(code, label, 0.0, "", "", "", "", 0, "", 0.0, RefreshAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshAssignmentStatus next)
    {
        switch (Status)
        {
            case RefreshAssignmentStatus.Open:
                return next == RefreshAssignmentStatus.Resolved || next == RefreshAssignmentStatus.Reopened;
            case RefreshAssignmentStatus.InReview:
                return next == RefreshAssignmentStatus.Reopened || next == RefreshAssignmentStatus.Archived;
            case RefreshAssignmentStatus.Resolved:
                return next == RefreshAssignmentStatus.Archived;
            case RefreshAssignmentStatus.Reopened:
                return next == RefreshAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RefreshAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"refresh assignment"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * Fte, 2);
    }

    public RefreshAssignment Copy()
    {
        return new RefreshAssignment(
            Code,
            Label,
            Budget,
            Badge,
            Team,
            Name,
            Department,
            BatchSize,
            Unit,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
