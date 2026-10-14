namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled audit plan is in its life cycle.</summary>
public enum ScheduledAuditPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled audit plan and its current status.</summary>
public sealed class ScheduledAuditPlan(string code, string name, int revision, bool approved, int durationMinutes, string ownerId, string externalRef, string grade, string location, int leaveDays, int quantity, ScheduledAuditPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string OwnerId { get; set; } = ownerId;
    public string ExternalRef { get; set; } = externalRef;
    public string Grade { get; set; } = grade;
    public string Location { get; set; } = location;
    public int LeaveDays { get; set; } = leaveDays;
    public int Quantity { get; set; } = quantity;
    public ScheduledAuditPlanStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled audit plan in status created.</summary>
    public static ScheduledAuditPlan NewScheduledAuditPlan(string code, string name)
    {
        return new ScheduledAuditPlan(
            code,
            name,
            0,
            false,
            0,
            "",
            "",
            "",
            "",
            0,
            0,
            ScheduledAuditPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAuditPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAuditPlanStatus next)
    {
        switch (Status)
        {
            case ScheduledAuditPlanStatus.Created:
                return next == ScheduledAuditPlanStatus.Retired;
            case ScheduledAuditPlanStatus.Validated:
                switch (next)
                {
                    case ScheduledAuditPlanStatus.Published:
                    case ScheduledAuditPlanStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledAuditPlanStatus.Published:
                return next == ScheduledAuditPlanStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledAuditPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled audit plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Revision + (double)DurationMinutes, 2);
    }

    public ScheduledAuditPlan Copy()
    {
        return new ScheduledAuditPlan(
            Code,
            Name,
            Revision,
            Approved,
            DurationMinutes,
            OwnerId,
            ExternalRef,
            Grade,
            Location,
            LeaveDays,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
