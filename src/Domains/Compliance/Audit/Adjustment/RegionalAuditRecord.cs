namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional audit record is in its life cycle.</summary>
public enum RegionalAuditRecordStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional audit record and its current status.</summary>
public sealed class RegionalAuditRecord(string code, string label, int leaveDays, int updatedAt, string department, int priority, string managerId, bool active, int startsAt, int durationMinutes, string badge, RegionalAuditRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Department { get; set; } = department;
    public int Priority { get; set; } = priority;
    public string ManagerId { get; set; } = managerId;
    public bool Active { get; set; } = active;
    public int StartsAt { get; set; } = startsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public RegionalAuditRecordStatus Status { get; set; } = status;

    /// <summary>Starts a regional audit record in status draft.</summary>
    public static RegionalAuditRecord NewRegionalAuditRecord(string code, string label)
    {
        return new RegionalAuditRecord(
            code,
            label,
            0,
            0,
            "",
            0,
            "",
            false,
            0,
            0,
            "",
            RegionalAuditRecordStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAuditRecordStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAuditRecordStatus next)
    {
        switch (Status)
        {
            case RegionalAuditRecordStatus.Draft:
                return next == RegionalAuditRecordStatus.Active || next == RegionalAuditRecordStatus.Suspended;
            case RegionalAuditRecordStatus.Pending:
                return next == RegionalAuditRecordStatus.Active || next == RegionalAuditRecordStatus.Closed;
            case RegionalAuditRecordStatus.Active:
                switch (next)
                {
                    case RegionalAuditRecordStatus.Closed:
                        return true;
                }
                return false;
            case RegionalAuditRecordStatus.Suspended:
                switch (next)
                {
                    case RegionalAuditRecordStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAuditRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional audit record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Priority + (double)DurationMinutes, 2);
    }

    public RegionalAuditRecord Copy()
    {
        return new RegionalAuditRecord(
            Code,
            Label,
            LeaveDays,
            UpdatedAt,
            Department,
            Priority,
            ManagerId,
            Active,
            StartsAt,
            DurationMinutes,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
