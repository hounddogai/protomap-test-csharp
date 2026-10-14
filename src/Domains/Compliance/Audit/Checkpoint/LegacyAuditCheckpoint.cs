namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy audit checkpoint is in its life cycle.</summary>
public enum LegacyAuditCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy audit checkpoint as the audit team keeps it.</summary>
public sealed class LegacyAuditCheckpoint(string code, string title, string grade, string timezone, string notes, string location, string currency, int quantity, LegacyAuditCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public int Quantity { get; set; } = quantity;
    public LegacyAuditCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a legacy audit checkpoint in status new.</summary>
    public static LegacyAuditCheckpoint NewLegacyAuditCheckpoint(string code, string title)
    {
        return new LegacyAuditCheckpoint(code, title, "", "", "", "", "", 0, LegacyAuditCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAuditCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAuditCheckpointStatus next)
    {
        switch (Status)
        {
            case LegacyAuditCheckpointStatus.New:
                switch (next)
                {
                    case LegacyAuditCheckpointStatus.Running:
                    case LegacyAuditCheckpointStatus.Done:
                        return true;
                }
                return false;
            case LegacyAuditCheckpointStatus.Queued:
                switch (next)
                {
                    case LegacyAuditCheckpointStatus.Done:
                        return true;
                }
                return false;
            case LegacyAuditCheckpointStatus.Running:
                switch (next)
                {
                    case LegacyAuditCheckpointStatus.Failed:
                        return true;
                }
                return false;
            case LegacyAuditCheckpointStatus.Done:
                switch (next)
                {
                    case LegacyAuditCheckpointStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAuditCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy audit checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public LegacyAuditCheckpoint Copy()
    {
        return new LegacyAuditCheckpoint(Code, Title, Grade, Timezone, Notes, Location, Currency, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
