namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit threshold is in its life cycle.</summary>
public enum AuditThresholdStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A audit threshold as the audit team keeps it.</summary>
public sealed class AuditThreshold(string code, string name, string source, int quantity, double maximum, string department, string label, bool pinned, string region, double minimum, int retries, AuditThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public int Quantity { get; set; } = quantity;
    public double Maximum { get; set; } = maximum;
    public string Department { get; set; } = department;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Region { get; set; } = region;
    public double Minimum { get; set; } = minimum;
    public int Retries { get; set; } = retries;
    public AuditThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a audit threshold in status draft.</summary>
    public static AuditThreshold NewAuditThreshold(string code, string name)
    {
        return new AuditThreshold(code, name, "", 0, 0.0, "", "", false, "", 0.0, 0, AuditThresholdStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditThresholdStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditThresholdStatus next)
    {
        switch (Status)
        {
            case AuditThresholdStatus.Draft:
                switch (next)
                {
                    case AuditThresholdStatus.Active:
                    case AuditThresholdStatus.Closed:
                        return true;
                }
                return false;
            case AuditThresholdStatus.Pending:
                return next == AuditThresholdStatus.Active;
            case AuditThresholdStatus.Active:
                return next == AuditThresholdStatus.Suspended;
            case AuditThresholdStatus.Suspended:
                return next == AuditThresholdStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AuditThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Quantity - (double)Retries, 2);
    }

    public AuditThreshold Copy()
    {
        return new AuditThreshold(
            Code,
            Name,
            Source,
            Quantity,
            Maximum,
            Department,
            Label,
            Pinned,
            Region,
            Minimum,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
