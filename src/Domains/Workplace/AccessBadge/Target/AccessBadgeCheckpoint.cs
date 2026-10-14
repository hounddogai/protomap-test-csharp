namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a access badge checkpoint is in its life cycle.</summary>
public enum AccessBadgeCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a access badge checkpoint and its current status.</summary>
public sealed class AccessBadgeCheckpoint(string code, string name, string channel, int batchSize, string unit, bool archived, string department, int updatedAt, string jobCode, AccessBadgeCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int BatchSize { get; set; } = batchSize;
    public string Unit { get; set; } = unit;
    public bool Archived { get; set; } = archived;
    public string Department { get; set; } = department;
    public int UpdatedAt { get; set; } = updatedAt;
    public string JobCode { get; set; } = jobCode;
    public AccessBadgeCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a access badge checkpoint in status new.</summary>
    public static AccessBadgeCheckpoint NewAccessBadgeCheckpoint(string code, string name)
    {
        return new AccessBadgeCheckpoint(code, name, "", 0, "", false, "", 0, "", AccessBadgeCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccessBadgeCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccessBadgeCheckpointStatus next)
    {
        switch (Status)
        {
            case AccessBadgeCheckpointStatus.New:
                return next == AccessBadgeCheckpointStatus.Queued || next == AccessBadgeCheckpointStatus.Failed;
            case AccessBadgeCheckpointStatus.Queued:
                switch (next)
                {
                    case AccessBadgeCheckpointStatus.Running:
                    case AccessBadgeCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case AccessBadgeCheckpointStatus.Running:
                return next == AccessBadgeCheckpointStatus.Failed;
            case AccessBadgeCheckpointStatus.Done:
                return next == AccessBadgeCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AccessBadgeCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"access badge checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize - (double)UpdatedAt, 2);
    }

    public AccessBadgeCheckpoint Copy()
    {
        return new AccessBadgeCheckpoint(
            Code,
            Name,
            Channel,
            BatchSize,
            Unit,
            Archived,
            Department,
            UpdatedAt,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
