namespace Acme.Workforce.Compliance.Audit.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default audit checkpoint is in its life cycle.</summary>
public enum DefaultAuditCheckpointStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A default audit checkpoint. Its code is unique within a tenant.</summary>
public sealed class DefaultAuditCheckpoint(string code, string name, string employeeId, string contactEmail, int startsAt, string category, string managerId, string timezone, string label, bool active, string source, DefaultAuditCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public string Label { get; set; } = label;
    public bool Active { get; set; } = active;
    public string Source { get; set; } = source;
    public DefaultAuditCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a default audit checkpoint in status open.</summary>
    public static DefaultAuditCheckpoint NewDefaultAuditCheckpoint(string code, string name)
    {
        return new DefaultAuditCheckpoint(
            code,
            name,
            "",
            "",
            0,
            "",
            "",
            "",
            "",
            false,
            "",
            DefaultAuditCheckpointStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultAuditCheckpointStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultAuditCheckpointStatus next)
    {
        switch (Status)
        {
            case DefaultAuditCheckpointStatus.Open:
                return next == DefaultAuditCheckpointStatus.InReview;
            case DefaultAuditCheckpointStatus.InReview:
                return next == DefaultAuditCheckpointStatus.Reopened || next == DefaultAuditCheckpointStatus.Archived;
            case DefaultAuditCheckpointStatus.Resolved:
                return next == DefaultAuditCheckpointStatus.Archived;
            case DefaultAuditCheckpointStatus.Reopened:
                switch (next)
                {
                    case DefaultAuditCheckpointStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultAuditCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default audit checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)StartsAt;
    }

    public DefaultAuditCheckpoint Copy()
    {
        return new DefaultAuditCheckpoint(
            Code,
            Name,
            EmployeeId,
            ContactEmail,
            StartsAt,
            Category,
            ManagerId,
            Timezone,
            Label,
            Active,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
