namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive audit baseline is in its life cycle.</summary>
public enum AdaptiveAuditBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A adaptive audit baseline as the audit team keeps it.</summary>
public sealed class AdaptiveAuditBaseline(string code, string name, string contactEmail, bool approved, double maximum, AdaptiveAuditBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public AdaptiveAuditBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive audit baseline in status planned.</summary>
    public static AdaptiveAuditBaseline NewAdaptiveAuditBaseline(string code, string name)
    {
        return new AdaptiveAuditBaseline(code, name, "", false, 0.0, AdaptiveAuditBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveAuditBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveAuditBaselineStatus next)
    {
        switch (Status)
        {
            case AdaptiveAuditBaselineStatus.Planned:
                switch (next)
                {
                    case AdaptiveAuditBaselineStatus.InProgress:
                        return true;
                }
                return false;
            case AdaptiveAuditBaselineStatus.Scheduled:
                return next == AdaptiveAuditBaselineStatus.InProgress;
            case AdaptiveAuditBaselineStatus.InProgress:
                return next == AdaptiveAuditBaselineStatus.Completed;
            case AdaptiveAuditBaselineStatus.Completed:
                return next == AdaptiveAuditBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveAuditBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive audit baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public AdaptiveAuditBaseline Copy()
    {
        return new AdaptiveAuditBaseline(Code, Name, ContactEmail, Approved, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
