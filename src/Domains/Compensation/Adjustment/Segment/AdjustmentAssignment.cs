namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment assignment is in its life cycle.</summary>
public enum AdjustmentAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A adjustment assignment as the adjustment team keeps it.</summary>
public sealed class AdjustmentAssignment(string code, string name, string country, string segment, int attempts, string timezone, int quantity, string category, int startsAt, AdjustmentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Segment { get; set; } = segment;
    public int Attempts { get; set; } = attempts;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public AdjustmentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment assignment in status idle.</summary>
    public static AdjustmentAssignment NewAdjustmentAssignment(string code, string name)
    {
        return new AdjustmentAssignment(code, name, "", "", 0, "", 0, "", 0, AdjustmentAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentAssignmentStatus next)
    {
        switch (Status)
        {
            case AdjustmentAssignmentStatus.Idle:
                switch (next)
                {
                    case AdjustmentAssignmentStatus.Warming:
                    case AdjustmentAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case AdjustmentAssignmentStatus.Warming:
                switch (next)
                {
                    case AdjustmentAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case AdjustmentAssignmentStatus.Ready:
                return next == AdjustmentAssignmentStatus.Draining || next == AdjustmentAssignmentStatus.Stopped;
            case AdjustmentAssignmentStatus.Draining:
                switch (next)
                {
                    case AdjustmentAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdjustmentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity * (double)StartsAt, 2);
    }

    public AdjustmentAssignment Copy()
    {
        return new AdjustmentAssignment(
            Code,
            Name,
            Country,
            Segment,
            Attempts,
            Timezone,
            Quantity,
            Category,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
