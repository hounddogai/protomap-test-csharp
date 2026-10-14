namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit quota is in its life cycle.</summary>
public enum AuditQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a audit quota and its current status.</summary>
public sealed class AuditQuota(string code, string name, string contactEmail, string category, string source, int version, Dictionary<string, string> attributes, double fte, bool approved, AuditQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Category { get; set; } = category;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public bool Approved { get; set; } = approved;
    public AuditQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a audit quota in status planned.</summary>
    public static AuditQuota NewAuditQuota(string code, string name)
    {
        return new AuditQuota(
            code,
            name,
            "",
            "",
            "",
            0,
            new Dictionary<string, string>(),
            0.0,
            false,
            AuditQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditQuotaStatus next)
    {
        switch (Status)
        {
            case AuditQuotaStatus.Planned:
                switch (next)
                {
                    case AuditQuotaStatus.InProgress:
                        return true;
                }
                return false;
            case AuditQuotaStatus.Scheduled:
                return next == AuditQuotaStatus.Cancelled;
            case AuditQuotaStatus.InProgress:
                return next == AuditQuotaStatus.Completed || next == AuditQuotaStatus.Cancelled;
            case AuditQuotaStatus.Completed:
                return next == AuditQuotaStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AuditQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double NetTotal()
    {
        return Math.Round(Fte + (double)Version, 2);
    }

    public AuditQuota Copy()
    {
        return new AuditQuota(
            Code,
            Name,
            ContactEmail,
            Category,
            Source,
            Version,
            Attributes,
            Fte,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
