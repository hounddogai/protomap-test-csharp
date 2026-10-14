namespace Acme.Workforce.Contractors.Timesheet.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy timesheet metric is in its life cycle.</summary>
public enum LegacyTimesheetMetricStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy timesheet metric as the timesheet team keeps it.</summary>
public sealed class LegacyTimesheetMetric(string code, string title, string ownerId, int endsAt, int createdAt, double fte, string phone, string segment, LegacyTimesheetMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public double Fte { get; set; } = fte;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public LegacyTimesheetMetricStatus Status { get; set; } = status;

    /// <summary>Starts a legacy timesheet metric in status proposed.</summary>
    public static LegacyTimesheetMetric NewLegacyTimesheetMetric(string code, string title)
    {
        return new LegacyTimesheetMetric(code, title, "", 0, 0, 0.0, "", "", LegacyTimesheetMetricStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTimesheetMetricStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTimesheetMetricStatus next)
    {
        switch (Status)
        {
            case LegacyTimesheetMetricStatus.Proposed:
                return next == LegacyTimesheetMetricStatus.Rejected;
            case LegacyTimesheetMetricStatus.Approved:
                return next == LegacyTimesheetMetricStatus.Rejected || next == LegacyTimesheetMetricStatus.Withdrawn;
            case LegacyTimesheetMetricStatus.Rejected:
                return next == LegacyTimesheetMetricStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyTimesheetMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy timesheet metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy timesheet metric"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)CreatedAt - Fte, 2);
    }

    public LegacyTimesheetMetric Copy()
    {
        return new LegacyTimesheetMetric(Code, Title, OwnerId, EndsAt, CreatedAt, Fte, Phone, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
