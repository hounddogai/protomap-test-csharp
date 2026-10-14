namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched audit template is in its life cycle.</summary>
public enum BatchedAuditTemplateStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched audit template. Its code is unique within a tenant.</summary>
public sealed class BatchedAuditTemplate(string code, string title, string segment, double ceiling, string label, double budget, string timezone, Dictionary<string, string> attributes, string grade, string employeeId, double score, BatchedAuditTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Ceiling { get; set; } = ceiling;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Timezone { get; set; } = timezone;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Grade { get; set; } = grade;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public BatchedAuditTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched audit template in status new.</summary>
    public static BatchedAuditTemplate NewBatchedAuditTemplate(string code, string title)
    {
        return new BatchedAuditTemplate(
            code,
            title,
            "",
            0.0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0.0,
            BatchedAuditTemplateStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAuditTemplateStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAuditTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedAuditTemplateStatus.New:
                switch (next)
                {
                    case BatchedAuditTemplateStatus.Running:
                        return true;
                }
                return false;
            case BatchedAuditTemplateStatus.Queued:
                switch (next)
                {
                    case BatchedAuditTemplateStatus.Running:
                    case BatchedAuditTemplateStatus.Done:
                        return true;
                }
                return false;
            case BatchedAuditTemplateStatus.Running:
                switch (next)
                {
                    case BatchedAuditTemplateStatus.Failed:
                        return true;
                }
                return false;
            case BatchedAuditTemplateStatus.Done:
                switch (next)
                {
                    case BatchedAuditTemplateStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAuditTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched audit template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
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

    public double AdjustedValue()
    {
        return Math.Round(Budget * Ceiling, 2);
    }

    public BatchedAuditTemplate Copy()
    {
        return new BatchedAuditTemplate(
            Code,
            Title,
            Segment,
            Ceiling,
            Label,
            Budget,
            Timezone,
            Attributes,
            Grade,
            EmployeeId,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
