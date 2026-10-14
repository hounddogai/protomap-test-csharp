namespace Acme.Workforce.Engagement.TownHall.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall assignment is in its life cycle.</summary>
public enum LegacyTownHallAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One legacy town hall assignment, with what the planners need to act on it.</summary>
public sealed class LegacyTownHallAssignment(string code, string label, string grade, string badge, int limit, int startsAt, int endsAt, int expiresAt, string employeeId, double score, string location, LegacyTownHallAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public string Location { get; set; } = location;
    public LegacyTownHallAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall assignment in status idle.</summary>
    public static LegacyTownHallAssignment NewLegacyTownHallAssignment(string code, string label)
    {
        return new LegacyTownHallAssignment(
            code,
            label,
            "",
            "",
            0,
            0,
            0,
            0,
            "",
            0.0,
            "",
            LegacyTownHallAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallAssignmentStatus.Idle:
                return next == LegacyTownHallAssignmentStatus.Ready;
            case LegacyTownHallAssignmentStatus.Warming:
                switch (next)
                {
                    case LegacyTownHallAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case LegacyTownHallAssignmentStatus.Ready:
                return next == LegacyTownHallAssignmentStatus.Draining || next == LegacyTownHallAssignmentStatus.Stopped;
            case LegacyTownHallAssignmentStatus.Draining:
                switch (next)
                {
                    case LegacyTownHallAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score - (double)Limit, 2);
    }

    public LegacyTownHallAssignment Copy()
    {
        return new LegacyTownHallAssignment(
            Code,
            Label,
            Grade,
            Badge,
            Limit,
            StartsAt,
            EndsAt,
            ExpiresAt,
            EmployeeId,
            Score,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
