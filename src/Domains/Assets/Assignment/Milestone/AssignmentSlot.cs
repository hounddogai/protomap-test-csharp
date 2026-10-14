namespace Acme.Workforce.Assets.Assignment.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment slot is in its life cycle.</summary>
public enum AssignmentSlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A assignment slot as the assignment team keeps it.</summary>
public sealed class AssignmentSlot(string code, string label, int batchSize, int createdAt, int priority, string ownerId, AssignmentSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int BatchSize { get; set; } = batchSize;
    public int CreatedAt { get; set; } = createdAt;
    public int Priority { get; set; } = priority;
    public string OwnerId { get; set; } = ownerId;
    public AssignmentSlotStatus Status { get; set; } = status;

    /// <summary>Starts a assignment slot in status requested.</summary>
    public static AssignmentSlot NewAssignmentSlot(string code, string label)
    {
        return new AssignmentSlot(code, label, 0, 0, 0, "", AssignmentSlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentSlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentSlotStatus next)
    {
        switch (Status)
        {
            case AssignmentSlotStatus.Requested:
                switch (next)
                {
                    case AssignmentSlotStatus.Revoked:
                        return true;
                }
                return false;
            case AssignmentSlotStatus.Granted:
                return next == AssignmentSlotStatus.Revoked || next == AssignmentSlotStatus.Expired;
            case AssignmentSlotStatus.Revoked:
                switch (next)
                {
                    case AssignmentSlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"assignment slot"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)CreatedAt - (double)Priority, 2);
    }

    public AssignmentSlot Copy()
    {
        return new AssignmentSlot(Code, Label, BatchSize, CreatedAt, Priority, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
