namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy audit manifest is in its life cycle.</summary>
public enum LegacyAuditManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy audit manifest. Its code is unique within a tenant.</summary>
public sealed class LegacyAuditManifest(string code, string name, string employeeId, int retries, int durationMinutes, int attempts, int capacity, string team, double fte, LegacyAuditManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Attempts { get; set; } = attempts;
    public int Capacity { get; set; } = capacity;
    public string Team { get; set; } = team;
    public double Fte { get; set; } = fte;
    public LegacyAuditManifestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy audit manifest in status new.</summary>
    public static LegacyAuditManifest NewLegacyAuditManifest(string code, string name)
    {
        return new LegacyAuditManifest(code, name, "", 0, 0, 0, 0, "", 0.0, LegacyAuditManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAuditManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAuditManifestStatus next)
    {
        switch (Status)
        {
            case LegacyAuditManifestStatus.New:
                return next == LegacyAuditManifestStatus.Queued || next == LegacyAuditManifestStatus.Running;
            case LegacyAuditManifestStatus.Queued:
                return next == LegacyAuditManifestStatus.Done || next == LegacyAuditManifestStatus.Failed;
            case LegacyAuditManifestStatus.Running:
                switch (next)
                {
                    case LegacyAuditManifestStatus.Done:
                    case LegacyAuditManifestStatus.Failed:
                        return true;
                }
                return false;
            case LegacyAuditManifestStatus.Done:
                switch (next)
                {
                    case LegacyAuditManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyAuditManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy audit manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)DurationMinutes * (double)Attempts, 2);
    }

    public LegacyAuditManifest Copy()
    {
        return new LegacyAuditManifest(
            Code,
            Name,
            EmployeeId,
            Retries,
            DurationMinutes,
            Attempts,
            Capacity,
            Team,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
