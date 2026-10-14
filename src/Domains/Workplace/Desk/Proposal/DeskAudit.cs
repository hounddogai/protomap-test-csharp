namespace Acme.Workforce.Workplace.Desk.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk audit is in its life cycle.</summary>
public enum DeskAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One desk audit, with what the planners need to act on it.</summary>
public sealed class DeskAudit(string code, string label, double minimum, int windowDays, string ownerId, string unit, int durationMinutes, string externalRef, double ceiling, string timezone, string managerId, DeskAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public int WindowDays { get; set; } = windowDays;
    public string OwnerId { get; set; } = ownerId;
    public string Unit { get; set; } = unit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ExternalRef { get; set; } = externalRef;
    public double Ceiling { get; set; } = ceiling;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public DeskAuditStatus Status { get; set; } = status;

    /// <summary>Starts a desk audit in status new.</summary>
    public static DeskAudit NewDeskAudit(string code, string label)
    {
        return new DeskAudit(code, label, 0.0, 0, "", "", 0, "", 0.0, "", "", DeskAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskAuditStatus next)
    {
        switch (Status)
        {
            case DeskAuditStatus.New:
                switch (next)
                {
                    case DeskAuditStatus.Queued:
                    case DeskAuditStatus.Done:
                        return true;
                }
                return false;
            case DeskAuditStatus.Queued:
                return next == DeskAuditStatus.Running || next == DeskAuditStatus.Done;
            case DeskAuditStatus.Running:
                switch (next)
                {
                    case DeskAuditStatus.Done:
                    case DeskAuditStatus.Failed:
                        return true;
                }
                return false;
            case DeskAuditStatus.Done:
                switch (next)
                {
                    case DeskAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays + Ceiling, 2);
    }

    public DeskAudit Copy()
    {
        return new DeskAudit(
            Code,
            Label,
            Minimum,
            WindowDays,
            OwnerId,
            Unit,
            DurationMinutes,
            ExternalRef,
            Ceiling,
            Timezone,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
