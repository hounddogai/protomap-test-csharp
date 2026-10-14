namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary pay run audit is in its life cycle.</summary>
public enum PrimaryPayRunAuditStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary pay run audit and its current status.</summary>
public sealed class PrimaryPayRunAudit(string code, string title, Dictionary<string, string> attributes, string country, string checksum, int windowDays, string source, int version, double weight, int durationMinutes, PrimaryPayRunAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Country { get; set; } = country;
    public string Checksum { get; set; } = checksum;
    public int WindowDays { get; set; } = windowDays;
    public string Source { get; set; } = source;
    public int Version { get; set; } = version;
    public double Weight { get; set; } = weight;
    public int DurationMinutes { get; set; } = durationMinutes;
    public PrimaryPayRunAuditStatus Status { get; set; } = status;

    /// <summary>Starts a primary pay run audit in status open.</summary>
    public static PrimaryPayRunAudit NewPrimaryPayRunAudit(string code, string title)
    {
        return new PrimaryPayRunAudit(
            code,
            title,
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            0,
            0.0,
            0,
            PrimaryPayRunAuditStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPayRunAuditStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPayRunAuditStatus next)
    {
        switch (Status)
        {
            case PrimaryPayRunAuditStatus.Open:
                return next == PrimaryPayRunAuditStatus.InReview || next == PrimaryPayRunAuditStatus.Resolved;
            case PrimaryPayRunAuditStatus.InReview:
                switch (next)
                {
                    case PrimaryPayRunAuditStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryPayRunAuditStatus.Resolved:
                switch (next)
                {
                    case PrimaryPayRunAuditStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryPayRunAuditStatus.Reopened:
                return next == PrimaryPayRunAuditStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryPayRunAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary pay run audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedValue()
    {
        return Math.Round(Weight * (double)WindowDays, 2);
    }

    public PrimaryPayRunAudit Copy()
    {
        return new PrimaryPayRunAudit(
            Code,
            Title,
            Attributes,
            Country,
            Checksum,
            WindowDays,
            Source,
            Version,
            Weight,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
