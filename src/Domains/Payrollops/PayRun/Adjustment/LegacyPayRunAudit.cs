namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay run audit is in its life cycle.</summary>
public enum LegacyPayRunAuditStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One legacy pay run audit, with what the planners need to act on it.</summary>
public sealed class LegacyPayRunAudit(string code, string name, double ceiling, string checksum, string skill, string segment, bool pinned, string source, int windowDays, int capacity, string region, LegacyPayRunAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string Checksum { get; set; } = checksum;
    public string Skill { get; set; } = skill;
    public string Segment { get; set; } = segment;
    public bool Pinned { get; set; } = pinned;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public string Region { get; set; } = region;
    public LegacyPayRunAuditStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay run audit in status draft.</summary>
    public static LegacyPayRunAudit NewLegacyPayRunAudit(string code, string name)
    {
        return new LegacyPayRunAudit(
            code,
            name,
            0.0,
            "",
            "",
            "",
            false,
            "",
            0,
            0,
            "",
            LegacyPayRunAuditStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayRunAuditStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayRunAuditStatus next)
    {
        switch (Status)
        {
            case LegacyPayRunAuditStatus.Draft:
                switch (next)
                {
                    case LegacyPayRunAuditStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPayRunAuditStatus.Pending:
                return next == LegacyPayRunAuditStatus.Active || next == LegacyPayRunAuditStatus.Suspended;
            case LegacyPayRunAuditStatus.Active:
                switch (next)
                {
                    case LegacyPayRunAuditStatus.Closed:
                        return true;
                }
                return false;
            case LegacyPayRunAuditStatus.Suspended:
                return next == LegacyPayRunAuditStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LegacyPayRunAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay run audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)WindowDays + Ceiling, 2);
    }

    public LegacyPayRunAudit Copy()
    {
        return new LegacyPayRunAudit(
            Code,
            Name,
            Ceiling,
            Checksum,
            Skill,
            Segment,
            Pinned,
            Source,
            WindowDays,
            Capacity,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
