namespace Acme.Workforce.Workplace.AccessBadge.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict access badge assignment is in its life cycle.</summary>
public enum StrictAccessBadgeAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One strict access badge assignment, with what the planners need to act on it.</summary>
public sealed class StrictAccessBadgeAssignment(string code, string name, int attempts, string team, int updatedAt, string skill, double budget, StrictAccessBadgeAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Team { get; set; } = team;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Skill { get; set; } = skill;
    public double Budget { get; set; } = budget;
    public StrictAccessBadgeAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a strict access badge assignment in status draft.</summary>
    public static StrictAccessBadgeAssignment NewStrictAccessBadgeAssignment(string code, string name)
    {
        return new StrictAccessBadgeAssignment(
            code,
            name,
            0,
            "",
            0,
            "",
            0.0,
            StrictAccessBadgeAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccessBadgeAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccessBadgeAssignmentStatus next)
    {
        switch (Status)
        {
            case StrictAccessBadgeAssignmentStatus.Draft:
                return next == StrictAccessBadgeAssignmentStatus.Pending;
            case StrictAccessBadgeAssignmentStatus.Pending:
                switch (next)
                {
                    case StrictAccessBadgeAssignmentStatus.Suspended:
                    case StrictAccessBadgeAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case StrictAccessBadgeAssignmentStatus.Active:
                switch (next)
                {
                    case StrictAccessBadgeAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case StrictAccessBadgeAssignmentStatus.Suspended:
                switch (next)
                {
                    case StrictAccessBadgeAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictAccessBadgeAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict access badge assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedValue()
    {
        return Math.Round((double)UpdatedAt * (double)Attempts, 2);
    }

    public StrictAccessBadgeAssignment Copy()
    {
        return new StrictAccessBadgeAssignment(Code, Name, Attempts, Team, UpdatedAt, Skill, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
