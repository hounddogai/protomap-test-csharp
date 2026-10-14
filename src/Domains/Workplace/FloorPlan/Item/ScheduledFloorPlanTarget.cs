namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled floor plan target is in its life cycle.</summary>
public enum ScheduledFloorPlanTargetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled floor plan target and its current status.</summary>
public sealed class ScheduledFloorPlanTarget(string code, string label, string category, string costCenter, string externalRef, string team, int createdAt, string channel, string locale, int leaveDays, double rate, ScheduledFloorPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string CostCenter { get; set; } = costCenter;
    public string ExternalRef { get; set; } = externalRef;
    public string Team { get; set; } = team;
    public int CreatedAt { get; set; } = createdAt;
    public string Channel { get; set; } = channel;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public ScheduledFloorPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled floor plan target in status created.</summary>
    public static ScheduledFloorPlanTarget NewScheduledFloorPlanTarget(string code, string label)
    {
        return new ScheduledFloorPlanTarget(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            "",
            "",
            0,
            0.0,
            ScheduledFloorPlanTargetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFloorPlanTargetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFloorPlanTargetStatus next)
    {
        switch (Status)
        {
            case ScheduledFloorPlanTargetStatus.Created:
                return next == ScheduledFloorPlanTargetStatus.Retired;
            case ScheduledFloorPlanTargetStatus.Validated:
                return next == ScheduledFloorPlanTargetStatus.Retired;
            case ScheduledFloorPlanTargetStatus.Published:
                switch (next)
                {
                    case ScheduledFloorPlanTargetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledFloorPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled floor plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Rate + (double)LeaveDays, 2);
    }

    public ScheduledFloorPlanTarget Copy()
    {
        return new ScheduledFloorPlanTarget(
            Code,
            Label,
            Category,
            CostCenter,
            ExternalRef,
            Team,
            CreatedAt,
            Channel,
            Locale,
            LeaveDays,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
