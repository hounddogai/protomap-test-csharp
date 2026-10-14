namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit checkpoint is in its life cycle.</summary>
public enum AuditCheckpointStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A audit checkpoint as the audit team keeps it.</summary>
public sealed class AuditCheckpoint(string code, string name, string country, string region, int leaveDays, AuditCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Region { get; set; } = region;
    public int LeaveDays { get; set; } = leaveDays;
    public AuditCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a audit checkpoint in status planned.</summary>
    public static AuditCheckpoint NewAuditCheckpoint(string code, string name)
    {
        return new AuditCheckpoint(code, name, "", "", 0, AuditCheckpointStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditCheckpointStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditCheckpointStatus next)
    {
        switch (Status)
        {
            case AuditCheckpointStatus.Planned:
                return next == AuditCheckpointStatus.InProgress;
            case AuditCheckpointStatus.Scheduled:
                switch (next)
                {
                    case AuditCheckpointStatus.InProgress:
                    case AuditCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditCheckpointStatus.InProgress:
                switch (next)
                {
                    case AuditCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditCheckpointStatus.Completed:
                switch (next)
                {
                    case AuditCheckpointStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public AuditCheckpoint Copy()
    {
        return new AuditCheckpoint(Code, Name, Country, Region, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
