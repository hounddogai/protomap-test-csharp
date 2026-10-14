namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor audit is in its life cycle.</summary>
public enum VisitorAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a visitor audit and its current status.</summary>
public sealed class VisitorAudit(string code, string label, string timezone, int graceDays, string managerId, double threshold, bool pinned, int createdAt, int priority, string summary, VisitorAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int GraceDays { get; set; } = graceDays;
    public string ManagerId { get; set; } = managerId;
    public double Threshold { get; set; } = threshold;
    public bool Pinned { get; set; } = pinned;
    public int CreatedAt { get; set; } = createdAt;
    public int Priority { get; set; } = priority;
    public string Summary { get; set; } = summary;
    public VisitorAuditStatus Status { get; set; } = status;

    /// <summary>Starts a visitor audit in status new.</summary>
    public static VisitorAudit NewVisitorAudit(string code, string label)
    {
        return new VisitorAudit(code, label, "", 0, "", 0.0, false, 0, 0, "", VisitorAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorAuditStatus next)
    {
        switch (Status)
        {
            case VisitorAuditStatus.New:
                switch (next)
                {
                    case VisitorAuditStatus.Queued:
                    case VisitorAuditStatus.Failed:
                        return true;
                }
                return false;
            case VisitorAuditStatus.Queued:
                return next == VisitorAuditStatus.Done || next == VisitorAuditStatus.Failed;
            case VisitorAuditStatus.Running:
                switch (next)
                {
                    case VisitorAuditStatus.Done:
                        return true;
                }
                return false;
            case VisitorAuditStatus.Done:
                return next == VisitorAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"visitor audit"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Threshold * (double)Priority, 2);
    }

    public VisitorAudit Copy()
    {
        return new VisitorAudit(
            Code,
            Label,
            Timezone,
            GraceDays,
            ManagerId,
            Threshold,
            Pinned,
            CreatedAt,
            Priority,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
