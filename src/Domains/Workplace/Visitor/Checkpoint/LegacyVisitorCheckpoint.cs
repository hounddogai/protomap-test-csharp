namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy visitor checkpoint is in its life cycle.</summary>
public enum LegacyVisitorCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One legacy visitor checkpoint, with what the planners need to act on it.</summary>
public sealed class LegacyVisitorCheckpoint(string code, string title, int durationMinutes, string department, double ratio, double score, double floorValue, string description, LegacyVisitorCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Department { get; set; } = department;
    public double Ratio { get; set; } = ratio;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public string Description { get; set; } = description;
    public LegacyVisitorCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy visitor checkpoint in status requested.</summary>
    public static LegacyVisitorCheckpoint NewLegacyVisitorCheckpoint(string code, string title)
    {
        return new LegacyVisitorCheckpoint(
            code,
            title,
            0,
            "",
            0.0,
            0.0,
            0.0,
            "",
            LegacyVisitorCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyVisitorCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyVisitorCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacyVisitorCheckpointStatus.Requested:
                return next == LegacyVisitorCheckpointStatus.Expired;
            case LegacyVisitorCheckpointStatus.Granted:
                switch (next)
                {
                    case LegacyVisitorCheckpointStatus.Expired:
                        return true;
                }
                return false;
            case LegacyVisitorCheckpointStatus.Revoked:
                switch (next)
                {
                    case LegacyVisitorCheckpointStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyVisitorCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy visitor checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Ratio - FloorValue, 2);
    }

    public LegacyVisitorCheckpoint Copy()
    {
        return new LegacyVisitorCheckpoint(
            Code,
            Title,
            DurationMinutes,
            Department,
            Ratio,
            Score,
            FloorValue,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
