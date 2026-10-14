namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay run audit is in its life cycle.</summary>
public enum ScheduledPayRunAuditStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a scheduled pay run audit and its current status.</summary>
public sealed class ScheduledPayRunAudit(string code, string label, string timezone, string unit, string salaryBand, int capacity, string contactEmail, bool pinned, ScheduledPayRunAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public string SalaryBand { get; set; } = salaryBand;
    public int Capacity { get; set; } = capacity;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Pinned { get; set; } = pinned;
    public ScheduledPayRunAuditStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay run audit in status idle.</summary>
    public static ScheduledPayRunAudit NewScheduledPayRunAudit(string code, string label)
    {
        return new ScheduledPayRunAudit(code, label, "", "", "", 0, "", false, ScheduledPayRunAuditStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayRunAuditStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayRunAuditStatus next)
    {
        switch (Status)
        {
            case ScheduledPayRunAuditStatus.Idle:
                switch (next)
                {
                    case ScheduledPayRunAuditStatus.Ready:
                    case ScheduledPayRunAuditStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledPayRunAuditStatus.Warming:
                return next == ScheduledPayRunAuditStatus.Ready || next == ScheduledPayRunAuditStatus.Draining;
            case ScheduledPayRunAuditStatus.Ready:
                switch (next)
                {
                    case ScheduledPayRunAuditStatus.Draining:
                    case ScheduledPayRunAuditStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPayRunAuditStatus.Draining:
                return next == ScheduledPayRunAuditStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledPayRunAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay run audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)Capacity;
    }

    public ScheduledPayRunAudit Copy()
    {
        return new ScheduledPayRunAudit(
            Code,
            Label,
            Timezone,
            Unit,
            SalaryBand,
            Capacity,
            ContactEmail,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
