namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit override is in its life cycle.</summary>
public enum AuditOverrideStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A audit override. Its code is unique within a tenant.</summary>
public sealed class AuditOverride(string code, string label, double rate, string employeeId, int priority, string region, bool active, int leaveDays, string managerId, int version, AuditOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string EmployeeId { get; set; } = employeeId;
    public int Priority { get; set; } = priority;
    public string Region { get; set; } = region;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public int Version { get; set; } = version;
    public AuditOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a audit override in status created.</summary>
    public static AuditOverride NewAuditOverride(string code, string label)
    {
        return new AuditOverride(code, label, 0.0, "", 0, "", false, 0, "", 0, AuditOverrideStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditOverrideStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditOverrideStatus next)
    {
        switch (Status)
        {
            case AuditOverrideStatus.Created:
                return next == AuditOverrideStatus.Retired;
            case AuditOverrideStatus.Validated:
                return next == AuditOverrideStatus.Published || next == AuditOverrideStatus.Retired;
            case AuditOverrideStatus.Published:
                return next == AuditOverrideStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AuditOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round((double)Priority - (double)LeaveDays, 2);
    }

    public AuditOverride Copy()
    {
        return new AuditOverride(
            Code,
            Label,
            Rate,
            EmployeeId,
            Priority,
            Region,
            Active,
            LeaveDays,
            ManagerId,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
