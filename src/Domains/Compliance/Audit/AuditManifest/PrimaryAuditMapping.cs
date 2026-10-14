namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary audit mapping is in its life cycle.</summary>
public enum PrimaryAuditMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A primary audit mapping as the audit team keeps it.</summary>
public sealed class PrimaryAuditMapping(string code, string label, string externalRef, bool active, int durationMinutes, string locale, string skill, string title, PrimaryAuditMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public bool Active { get; set; } = active;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Locale { get; set; } = locale;
    public string Skill { get; set; } = skill;
    public string Title { get; set; } = title;
    public PrimaryAuditMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary audit mapping in status planned.</summary>
    public static PrimaryAuditMapping NewPrimaryAuditMapping(string code, string label)
    {
        return new PrimaryAuditMapping(code, label, "", false, 0, "", "", "", PrimaryAuditMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAuditMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAuditMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryAuditMappingStatus.Planned:
                switch (next)
                {
                    case PrimaryAuditMappingStatus.Scheduled:
                    case PrimaryAuditMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryAuditMappingStatus.Scheduled:
                switch (next)
                {
                    case PrimaryAuditMappingStatus.Completed:
                        return true;
                }
                return false;
            case PrimaryAuditMappingStatus.InProgress:
                switch (next)
                {
                    case PrimaryAuditMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimaryAuditMappingStatus.Completed:
                return next == PrimaryAuditMappingStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimaryAuditMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary audit mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public PrimaryAuditMapping Copy()
    {
        return new PrimaryAuditMapping(
            Code,
            Label,
            ExternalRef,
            Active,
            DurationMinutes,
            Locale,
            Skill,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
