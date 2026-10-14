namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict access badge audit is in its life cycle.</summary>
public enum StrictAccessBadgeAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A strict access badge audit. Its code is unique within a tenant.</summary>
public sealed class StrictAccessBadgeAudit(string code, string label, string badge, string description, double threshold, StrictAccessBadgeAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public StrictAccessBadgeAuditStatus Status { get; set; } = status;

    /// <summary>Starts a strict access badge audit in status draft.</summary>
    public static StrictAccessBadgeAudit NewStrictAccessBadgeAudit(string code, string label)
    {
        return new StrictAccessBadgeAudit(code, label, "", "", 0.0, StrictAccessBadgeAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAccessBadgeAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAccessBadgeAuditStatus next)
    {
        switch (Status)
        {
            case StrictAccessBadgeAuditStatus.Draft:
                return next == StrictAccessBadgeAuditStatus.Pending;
            case StrictAccessBadgeAuditStatus.Pending:
                switch (next)
                {
                    case StrictAccessBadgeAuditStatus.Active:
                        return true;
                }
                return false;
            case StrictAccessBadgeAuditStatus.Active:
                switch (next)
                {
                    case StrictAccessBadgeAuditStatus.Suspended:
                    case StrictAccessBadgeAuditStatus.Closed:
                        return true;
                }
                return false;
            case StrictAccessBadgeAuditStatus.Suspended:
                return next == StrictAccessBadgeAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StrictAccessBadgeAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict access badge audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Threshold;
    }

    public StrictAccessBadgeAudit Copy()
    {
        return new StrictAccessBadgeAudit(Code, Label, Badge, Description, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
