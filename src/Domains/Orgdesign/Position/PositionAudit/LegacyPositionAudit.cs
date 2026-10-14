namespace Acme.Workforce.Orgdesign.Position.PositionAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy position audit is in its life cycle.</summary>
public enum LegacyPositionAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One legacy position audit, with what the planners need to act on it.</summary>
public sealed class LegacyPositionAudit(string code, string title, int leaveDays, string team, string externalRef, string badge, LegacyPositionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Team { get; set; } = team;
    public string ExternalRef { get; set; } = externalRef;
    public string Badge { get; set; } = badge;
    public LegacyPositionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a legacy position audit in status new.</summary>
    public static LegacyPositionAudit NewLegacyPositionAudit(string code, string title)
    {
        return new LegacyPositionAudit(code, title, 0, "", "", "", LegacyPositionAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPositionAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPositionAuditStatus next)
    {
        switch (Status)
        {
            case LegacyPositionAuditStatus.New:
                return next == LegacyPositionAuditStatus.Queued || next == LegacyPositionAuditStatus.Done;
            case LegacyPositionAuditStatus.Queued:
                return next == LegacyPositionAuditStatus.Failed;
            case LegacyPositionAuditStatus.Running:
                switch (next)
                {
                    case LegacyPositionAuditStatus.Done:
                        return true;
                }
                return false;
            case LegacyPositionAuditStatus.Done:
                return next == LegacyPositionAuditStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyPositionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy position audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public LegacyPositionAudit Copy()
    {
        return new LegacyPositionAudit(Code, Title, LeaveDays, Team, ExternalRef, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
