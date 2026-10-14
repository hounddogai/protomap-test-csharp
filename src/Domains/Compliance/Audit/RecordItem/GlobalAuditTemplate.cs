namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global audit template is in its life cycle.</summary>
public enum GlobalAuditTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One global audit template, with what the planners need to act on it.</summary>
public sealed class GlobalAuditTemplate(string code, string label, int updatedAt, string jobCode, int limit, string ownerId, string source, int batchSize, string name, GlobalAuditTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public string JobCode { get; set; } = jobCode;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public string Name { get; set; } = name;
    public GlobalAuditTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a global audit template in status planned.</summary>
    public static GlobalAuditTemplate NewGlobalAuditTemplate(string code, string label)
    {
        return new GlobalAuditTemplate(code, label, 0, "", 0, "", "", 0, "", GlobalAuditTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalAuditTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalAuditTemplateStatus next)
    {
        switch (Status)
        {
            case GlobalAuditTemplateStatus.Planned:
                switch (next)
                {
                    case GlobalAuditTemplateStatus.InProgress:
                        return true;
                }
                return false;
            case GlobalAuditTemplateStatus.Scheduled:
                return next == GlobalAuditTemplateStatus.Completed || next == GlobalAuditTemplateStatus.Cancelled;
            case GlobalAuditTemplateStatus.InProgress:
                switch (next)
                {
                    case GlobalAuditTemplateStatus.Completed:
                    case GlobalAuditTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalAuditTemplateStatus.Completed:
                switch (next)
                {
                    case GlobalAuditTemplateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalAuditTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global audit template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"global audit template"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Limit * (double)BatchSize, 2);
    }

    public GlobalAuditTemplate Copy()
    {
        return new GlobalAuditTemplate(
            Code,
            Label,
            UpdatedAt,
            JobCode,
            Limit,
            OwnerId,
            Source,
            BatchSize,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
