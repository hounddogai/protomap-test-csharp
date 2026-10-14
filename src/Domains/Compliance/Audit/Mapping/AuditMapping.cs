namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit mapping is in its life cycle.</summary>
public enum AuditMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A audit mapping as the audit team keeps it.</summary>
public sealed class AuditMapping(string code, string name, string timezone, string managerId, string currency, int sequence, string segment, int retries, string ownerId, string phone, int graceDays, AuditMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public string ManagerId { get; set; } = managerId;
    public string Currency { get; set; } = currency;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public int Retries { get; set; } = retries;
    public string OwnerId { get; set; } = ownerId;
    public string Phone { get; set; } = phone;
    public int GraceDays { get; set; } = graceDays;
    public AuditMappingStatus Status { get; set; } = status;

    /// <summary>Starts a audit mapping in status planned.</summary>
    public static AuditMapping NewAuditMapping(string code, string name)
    {
        return new AuditMapping(code, name, "", "", "", 0, "", 0, "", "", 0, AuditMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditMappingStatus next)
    {
        switch (Status)
        {
            case AuditMappingStatus.Planned:
                switch (next)
                {
                    case AuditMappingStatus.Scheduled:
                    case AuditMappingStatus.Completed:
                        return true;
                }
                return false;
            case AuditMappingStatus.Scheduled:
                switch (next)
                {
                    case AuditMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditMappingStatus.InProgress:
                switch (next)
                {
                    case AuditMappingStatus.Cancelled:
                        return true;
                }
                return false;
            case AuditMappingStatus.Completed:
                switch (next)
                {
                    case AuditMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit mapping"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Retries + (double)Sequence, 2);
    }

    public AuditMapping Copy()
    {
        return new AuditMapping(
            Code,
            Name,
            Timezone,
            ManagerId,
            Currency,
            Sequence,
            Segment,
            Retries,
            OwnerId,
            Phone,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
