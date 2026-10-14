namespace Acme.Workforce.Compliance.Audit.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary audit schedule is in its life cycle.</summary>
public enum PrimaryAuditScheduleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One primary audit schedule, with what the planners need to act on it.</summary>
public sealed class PrimaryAuditSchedule(string code, string name, string description, int graceDays, double ratio, string employeeId, int limit, int startsAt, double minimum, string notes, double rate, PrimaryAuditScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Description { get; set; } = description;
    public int GraceDays { get; set; } = graceDays;
    public double Ratio { get; set; } = ratio;
    public string EmployeeId { get; set; } = employeeId;
    public int Limit { get; set; } = limit;
    public int StartsAt { get; set; } = startsAt;
    public double Minimum { get; set; } = minimum;
    public string Notes { get; set; } = notes;
    public double Rate { get; set; } = rate;
    public PrimaryAuditScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a primary audit schedule in status new.</summary>
    public static PrimaryAuditSchedule NewPrimaryAuditSchedule(string code, string name)
    {
        return new PrimaryAuditSchedule(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0,
            0,
            0.0,
            "",
            0.0,
            PrimaryAuditScheduleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAuditScheduleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAuditScheduleStatus next)
    {
        switch (Status)
        {
            case PrimaryAuditScheduleStatus.New:
                switch (next)
                {
                    case PrimaryAuditScheduleStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryAuditScheduleStatus.Queued:
                switch (next)
                {
                    case PrimaryAuditScheduleStatus.Running:
                        return true;
                }
                return false;
            case PrimaryAuditScheduleStatus.Running:
                switch (next)
                {
                    case PrimaryAuditScheduleStatus.Done:
                        return true;
                }
                return false;
            case PrimaryAuditScheduleStatus.Done:
                switch (next)
                {
                    case PrimaryAuditScheduleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAuditScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary audit schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary audit schedule"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Rate * (double)Limit, 2);
    }

    public PrimaryAuditSchedule Copy()
    {
        return new PrimaryAuditSchedule(
            Code,
            Name,
            Description,
            GraceDays,
            Ratio,
            EmployeeId,
            Limit,
            StartsAt,
            Minimum,
            Notes,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
