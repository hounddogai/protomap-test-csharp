namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit template is in its life cycle.</summary>
public enum AuditTemplateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A audit template. Its code is unique within a tenant.</summary>
public sealed class AuditTemplate(string code, string title, string skill, string unit, bool approved, string reviewer, int quantity, AuditTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public bool Approved { get; set; } = approved;
    public string Reviewer { get; set; } = reviewer;
    public int Quantity { get; set; } = quantity;
    public AuditTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a audit template in status idle.</summary>
    public static AuditTemplate NewAuditTemplate(string code, string title)
    {
        return new AuditTemplate(code, title, "", "", false, "", 0, AuditTemplateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditTemplateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditTemplateStatus next)
    {
        switch (Status)
        {
            case AuditTemplateStatus.Idle:
                switch (next)
                {
                    case AuditTemplateStatus.Draining:
                        return true;
                }
                return false;
            case AuditTemplateStatus.Warming:
                switch (next)
                {
                    case AuditTemplateStatus.Ready:
                        return true;
                }
                return false;
            case AuditTemplateStatus.Ready:
                return next == AuditTemplateStatus.Draining || next == AuditTemplateStatus.Stopped;
            case AuditTemplateStatus.Draining:
                return next == AuditTemplateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AuditTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public AuditTemplate Copy()
    {
        return new AuditTemplate(Code, Title, Skill, Unit, Approved, Reviewer, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
