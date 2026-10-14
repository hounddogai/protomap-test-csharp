namespace Acme.Workforce.Orgdesign.Position.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position assignment is in its life cycle.</summary>
public enum LegacyPositionAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a legacy position assignment and its current status.</summary>
public sealed class LegacyPositionAssignment(string code, string title, int retries, string jobCode, int expiresAt, string skill, string location, double maximum, string label, bool archived, LegacyPositionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public string JobCode { get; set; } = jobCode;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Skill { get; set; } = skill;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public LegacyPositionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position assignment in status draft.</summary>
    public static LegacyPositionAssignment NewLegacyPositionAssignment(string code, string title)
    {
        return new LegacyPositionAssignment(
            code,
            title,
            0,
            "",
            0,
            "",
            "",
            0.0,
            "",
            false,
            LegacyPositionAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionAssignmentStatus next)
    {
        switch (Status)
        {
            case LegacyPositionAssignmentStatus.Draft:
                switch (next)
                {
                    case LegacyPositionAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPositionAssignmentStatus.Pending:
                switch (next)
                {
                    case LegacyPositionAssignmentStatus.Active:
                    case LegacyPositionAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case LegacyPositionAssignmentStatus.Active:
                switch (next)
                {
                    case LegacyPositionAssignmentStatus.Suspended:
                    case LegacyPositionAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPositionAssignmentStatus.Suspended:
                return next == LegacyPositionAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyPositionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy position assignment"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)ExpiresAt + Maximum, 2);
    }

    public LegacyPositionAssignment Copy()
    {
        return new LegacyPositionAssignment(
            Code,
            Title,
            Retries,
            JobCode,
            ExpiresAt,
            Skill,
            Location,
            Maximum,
            Label,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
