namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit schedule is in its life cycle.</summary>
public enum AuditScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A audit schedule as the audit team keeps it.</summary>
public sealed class AuditSchedule(string code, string title, string category, int batchSize, double threshold, string locale, Dictionary<string, string> attributes, int tenureMonths, AuditScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int BatchSize { get; set; } = batchSize;
    public double Threshold { get; set; } = threshold;
    public string Locale { get; set; } = locale;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int TenureMonths { get; set; } = tenureMonths;
    public AuditScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a audit schedule in status proposed.</summary>
    public static AuditSchedule NewAuditSchedule(string code, string title)
    {
        return new AuditSchedule(
            code,
            title,
            "",
            0,
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            AuditScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditScheduleStatus next)
    {
        switch (Status)
        {
            case AuditScheduleStatus.Proposed:
                switch (next)
                {
                    case AuditScheduleStatus.Approved:
                    case AuditScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case AuditScheduleStatus.Approved:
                switch (next)
                {
                    case AuditScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case AuditScheduleStatus.Rejected:
                return next == AuditScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AuditScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"audit schedule"} {Code}";
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

    public double WeightedValue()
    {
        return Math.Round((double)TenureMonths * Threshold, 2);
    }

    public AuditSchedule Copy()
    {
        return new AuditSchedule(
            Code,
            Title,
            Category,
            BatchSize,
            Threshold,
            Locale,
            Attributes,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
