namespace Acme.Workforce.Compliance.Audit.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal audit assignment is in its life cycle.</summary>
public enum SeasonalAuditAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One seasonal audit assignment, with what the planners need to act on it.</summary>
public sealed class SeasonalAuditAssignment(string code, string name, string badge, int durationMinutes, int quantity, bool pinned, string unit, SeasonalAuditAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Quantity { get; set; } = quantity;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public SeasonalAuditAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal audit assignment in status draft.</summary>
    public static SeasonalAuditAssignment NewSeasonalAuditAssignment(string code, string name)
    {
        return new SeasonalAuditAssignment(code, name, "", 0, 0, false, "", SeasonalAuditAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalAuditAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalAuditAssignmentStatus next)
    {
        switch (Status)
        {
            case SeasonalAuditAssignmentStatus.Draft:
                return next == SeasonalAuditAssignmentStatus.Pending || next == SeasonalAuditAssignmentStatus.Closed;
            case SeasonalAuditAssignmentStatus.Pending:
                switch (next)
                {
                    case SeasonalAuditAssignmentStatus.Active:
                    case SeasonalAuditAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalAuditAssignmentStatus.Active:
                switch (next)
                {
                    case SeasonalAuditAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case SeasonalAuditAssignmentStatus.Suspended:
                return next == SeasonalAuditAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SeasonalAuditAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal audit assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedValue()
    {
        return Math.Round((double)DurationMinutes + (double)Quantity, 2);
    }

    public SeasonalAuditAssignment Copy()
    {
        return new SeasonalAuditAssignment(Code, Name, Badge, DurationMinutes, Quantity, Pinned, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
