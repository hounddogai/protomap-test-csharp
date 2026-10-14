namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy assignment baseline is in its life cycle.</summary>
public enum LegacyAssignmentBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A legacy assignment baseline as the assignment team keeps it.</summary>
public sealed class LegacyAssignmentBaseline(string code, string title, string summary, double floorValue, string timezone, int startsAt, bool visible, string segment, string externalRef, int durationMinutes, string skill, LegacyAssignmentBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public double FloorValue { get; set; } = floorValue;
    public string Timezone { get; set; } = timezone;
    public int StartsAt { get; set; } = startsAt;
    public bool Visible { get; set; } = visible;
    public string Segment { get; set; } = segment;
    public string ExternalRef { get; set; } = externalRef;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Skill { get; set; } = skill;
    public LegacyAssignmentBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a legacy assignment baseline in status proposed.</summary>
    public static LegacyAssignmentBaseline NewLegacyAssignmentBaseline(string code, string title)
    {
        return new LegacyAssignmentBaseline(
            code,
            title,
            "",
            0.0,
            "",
            0,
            false,
            "",
            "",
            0,
            "",
            LegacyAssignmentBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyAssignmentBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyAssignmentBaselineStatus next)
    {
        switch (Status)
        {
            case LegacyAssignmentBaselineStatus.Proposed:
                switch (next)
                {
                    case LegacyAssignmentBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyAssignmentBaselineStatus.Approved:
                return next == LegacyAssignmentBaselineStatus.Rejected || next == LegacyAssignmentBaselineStatus.Withdrawn;
            case LegacyAssignmentBaselineStatus.Rejected:
                return next == LegacyAssignmentBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyAssignmentBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy assignment baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"legacy assignment baseline"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)DurationMinutes + FloorValue, 2);
    }

    public LegacyAssignmentBaseline Copy()
    {
        return new LegacyAssignmentBaseline(
            Code,
            Title,
            Summary,
            FloorValue,
            Timezone,
            StartsAt,
            Visible,
            Segment,
            ExternalRef,
            DurationMinutes,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
