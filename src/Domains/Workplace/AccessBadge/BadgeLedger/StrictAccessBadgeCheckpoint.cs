namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict access badge checkpoint is in its life cycle.</summary>
public enum StrictAccessBadgeCheckpointStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One strict access badge checkpoint, with what the planners need to act on it.</summary>
public sealed class StrictAccessBadgeCheckpoint(string code, string name, int createdAt, double cost, string notes, double discount, string managerId, string segment, double fte, StrictAccessBadgeCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public double Cost { get; set; } = cost;
    public string Notes { get; set; } = notes;
    public double Discount { get; set; } = discount;
    public string ManagerId { get; set; } = managerId;
    public string Segment { get; set; } = segment;
    public double Fte { get; set; } = fte;
    public StrictAccessBadgeCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a strict access badge checkpoint in status requested.</summary>
    public static StrictAccessBadgeCheckpoint NewStrictAccessBadgeCheckpoint(string code, string name)
    {
        return new StrictAccessBadgeCheckpoint(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            0.0,
            StrictAccessBadgeCheckpointStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccessBadgeCheckpointStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccessBadgeCheckpointStatus next)
    {
        switch (Status)
        {
            case StrictAccessBadgeCheckpointStatus.Requested:
                return next == StrictAccessBadgeCheckpointStatus.Granted || next == StrictAccessBadgeCheckpointStatus.Revoked;
            case StrictAccessBadgeCheckpointStatus.Granted:
                return next == StrictAccessBadgeCheckpointStatus.Revoked || next == StrictAccessBadgeCheckpointStatus.Expired;
            case StrictAccessBadgeCheckpointStatus.Revoked:
                return next == StrictAccessBadgeCheckpointStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StrictAccessBadgeCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict access badge checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Cost * Discount, 2);
    }

    public StrictAccessBadgeCheckpoint Copy()
    {
        return new StrictAccessBadgeCheckpoint(
            Code,
            Name,
            CreatedAt,
            Cost,
            Notes,
            Discount,
            ManagerId,
            Segment,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
