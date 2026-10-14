namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered mobility audit is in its life cycle.</summary>
public enum TieredMobilityAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered mobility audit. Its code is unique within a tenant.</summary>
public sealed class TieredMobilityAudit(string code, string name, bool active, int sequence, int updatedAt, string employeeId, string currency, List<string> tags, double threshold, TieredMobilityAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Active { get; set; } = active;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public string EmployeeId { get; set; } = employeeId;
    public string Currency { get; set; } = currency;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public TieredMobilityAuditStatus Status { get; set; } = status;

    /// <summary>Starts a tiered mobility audit in status draft.</summary>
    public static TieredMobilityAudit NewTieredMobilityAudit(string code, string name)
    {
        return new TieredMobilityAudit(
            code,
            name,
            false,
            0,
            0,
            "",
            "",
            new List<string>(),
            0.0,
            TieredMobilityAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredMobilityAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredMobilityAuditStatus next)
    {
        switch (Status)
        {
            case TieredMobilityAuditStatus.Draft:
                switch (next)
                {
                    case TieredMobilityAuditStatus.Active:
                        return true;
                }
                return false;
            case TieredMobilityAuditStatus.Pending:
                return next == TieredMobilityAuditStatus.Suspended || next == TieredMobilityAuditStatus.Closed;
            case TieredMobilityAuditStatus.Active:
                switch (next)
                {
                    case TieredMobilityAuditStatus.Suspended:
                        return true;
                }
                return false;
            case TieredMobilityAuditStatus.Suspended:
                switch (next)
                {
                    case TieredMobilityAuditStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredMobilityAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered mobility audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered mobility audit"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)UpdatedAt - Threshold, 2);
    }

    public TieredMobilityAudit Copy()
    {
        return new TieredMobilityAudit(
            Code,
            Name,
            Active,
            Sequence,
            UpdatedAt,
            EmployeeId,
            Currency,
            Tags,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
