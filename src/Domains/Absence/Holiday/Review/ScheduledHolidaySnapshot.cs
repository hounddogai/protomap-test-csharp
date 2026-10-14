namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled holiday snapshot is in its life cycle.</summary>
public enum ScheduledHolidaySnapshotStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a scheduled holiday snapshot and its current status.</summary>
public sealed class ScheduledHolidaySnapshot(string code, string label, double hours, string country, int updatedAt, int priority, int version, string region, string summary, string description, double maximum, ScheduledHolidaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Country { get; set; } = country;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Priority { get; set; } = priority;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public double Maximum { get; set; } = maximum;
    public ScheduledHolidaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled holiday snapshot in status created.</summary>
    public static ScheduledHolidaySnapshot NewScheduledHolidaySnapshot(string code, string label)
    {
        return new ScheduledHolidaySnapshot(
            code,
            label,
            0.0,
            "",
            0,
            0,
            0,
            "",
            "",
            "",
            0.0,
            ScheduledHolidaySnapshotStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledHolidaySnapshotStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledHolidaySnapshotStatus next)
    {
        switch (Status)
        {
            case ScheduledHolidaySnapshotStatus.Created:
                switch (next)
                {
                    case ScheduledHolidaySnapshotStatus.Published:
                    case ScheduledHolidaySnapshotStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledHolidaySnapshotStatus.Validated:
                return next == ScheduledHolidaySnapshotStatus.Published || next == ScheduledHolidaySnapshotStatus.Retired;
            case ScheduledHolidaySnapshotStatus.Published:
                return next == ScheduledHolidaySnapshotStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledHolidaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled holiday snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Hours + Maximum, 2);
    }

    public ScheduledHolidaySnapshot Copy()
    {
        return new ScheduledHolidaySnapshot(
            Code,
            Label,
            Hours,
            Country,
            UpdatedAt,
            Priority,
            Version,
            Region,
            Summary,
            Description,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
