namespace Acme.Workforce.Orgdesign.Position.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position audit is in its life cycle.</summary>
public enum PositionAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A position audit as the position team keeps it.</summary>
public sealed class PositionAudit(string code, string title, string reviewer, int attempts, bool visible, PositionAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public int Attempts { get; set; } = attempts;
    public bool Visible { get; set; } = visible;
    public PositionAuditStatus Status { get; set; } = status;

    /// <summary>Starts a position audit in status draft.</summary>
    public static PositionAudit NewPositionAudit(string code, string title)
    {
        return new PositionAudit(code, title, "", 0, false, PositionAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionAuditStatus next)
    {
        switch (Status)
        {
            case PositionAuditStatus.Draft:
                return next == PositionAuditStatus.Active;
            case PositionAuditStatus.Pending:
                return next == PositionAuditStatus.Active;
            case PositionAuditStatus.Active:
                return next == PositionAuditStatus.Suspended || next == PositionAuditStatus.Closed;
            case PositionAuditStatus.Suspended:
                return next == PositionAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PositionAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public PositionAudit Copy()
    {
        return new PositionAudit(Code, Title, Reviewer, Attempts, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
