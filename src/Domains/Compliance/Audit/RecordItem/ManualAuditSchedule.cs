namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual audit schedule is in its life cycle.</summary>
public enum ManualAuditScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A manual audit schedule. Its code is unique within a tenant.</summary>
public sealed class ManualAuditSchedule(string code, string title, int revision, string badge, bool active, string locale, string unit, double discount, int endsAt, ManualAuditScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Badge { get; set; } = badge;
    public bool Active { get; set; } = active;
    public string Locale { get; set; } = locale;
    public string Unit { get; set; } = unit;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public ManualAuditScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a manual audit schedule in status idle.</summary>
    public static ManualAuditSchedule NewManualAuditSchedule(string code, string title)
    {
        return new ManualAuditSchedule(code, title, 0, "", false, "", "", 0.0, 0, ManualAuditScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAuditScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAuditScheduleStatus next)
    {
        switch (Status)
        {
            case ManualAuditScheduleStatus.Idle:
                switch (next)
                {
                    case ManualAuditScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case ManualAuditScheduleStatus.Warming:
                return next == ManualAuditScheduleStatus.Ready || next == ManualAuditScheduleStatus.Stopped;
            case ManualAuditScheduleStatus.Ready:
                switch (next)
                {
                    case ManualAuditScheduleStatus.Draining:
                    case ManualAuditScheduleStatus.Stopped:
                        return true;
                }
                return false;
            case ManualAuditScheduleStatus.Draining:
                return next == ManualAuditScheduleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ManualAuditScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual audit schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"manual audit schedule"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Discount + (double)Revision, 2);
    }

    public ManualAuditSchedule Copy()
    {
        return new ManualAuditSchedule(Code, Title, Revision, Badge, Active, Locale, Unit, Discount, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
