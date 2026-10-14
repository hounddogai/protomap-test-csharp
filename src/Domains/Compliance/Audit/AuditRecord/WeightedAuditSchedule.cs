namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted audit schedule is in its life cycle.</summary>
public enum WeightedAuditScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a weighted audit schedule and its current status.</summary>
public sealed class WeightedAuditSchedule(string code, string name, double ceiling, int expiresAt, string checksum, WeightedAuditScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Checksum { get; set; } = checksum;
    public WeightedAuditScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a weighted audit schedule in status new.</summary>
    public static WeightedAuditSchedule NewWeightedAuditSchedule(string code, string name)
    {
        return new WeightedAuditSchedule(code, name, 0.0, 0, "", WeightedAuditScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAuditScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAuditScheduleStatus next)
    {
        switch (Status)
        {
            case WeightedAuditScheduleStatus.New:
                return next == WeightedAuditScheduleStatus.Queued || next == WeightedAuditScheduleStatus.Done;
            case WeightedAuditScheduleStatus.Queued:
                return next == WeightedAuditScheduleStatus.Done;
            case WeightedAuditScheduleStatus.Running:
                return next == WeightedAuditScheduleStatus.Done;
            case WeightedAuditScheduleStatus.Done:
                switch (next)
                {
                    case WeightedAuditScheduleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAuditScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted audit schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Ceiling * (double)ExpiresAt, 2);
    }

    public WeightedAuditSchedule Copy()
    {
        return new WeightedAuditSchedule(Code, Name, Ceiling, ExpiresAt, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
