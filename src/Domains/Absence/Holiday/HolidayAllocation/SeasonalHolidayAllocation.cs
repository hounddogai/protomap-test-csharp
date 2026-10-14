namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday allocation is in its life cycle.</summary>
public enum SeasonalHolidayAllocationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A seasonal holiday allocation as the holiday team keeps it.</summary>
public sealed class SeasonalHolidayAllocation(string code, string name, string notes, bool archived, string description, string team, double ceiling, string location, string checksum, SeasonalHolidayAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Notes { get; set; } = notes;
    public bool Archived { get; set; } = archived;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public double Ceiling { get; set; } = ceiling;
    public string Location { get; set; } = location;
    public string Checksum { get; set; } = checksum;
    public SeasonalHolidayAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday allocation in status open.</summary>
    public static SeasonalHolidayAllocation NewSeasonalHolidayAllocation(string code, string name)
    {
        return new SeasonalHolidayAllocation(
            code,
            name,
            "",
            false,
            "",
            "",
            0.0,
            "",
            "",
            SeasonalHolidayAllocationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayAllocationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayAllocationStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayAllocationStatus.Open:
                switch (next)
                {
                    case SeasonalHolidayAllocationStatus.InReview:
                    case SeasonalHolidayAllocationStatus.Reopened:
                        return true;
                }
                return false;
            case SeasonalHolidayAllocationStatus.InReview:
                return next == SeasonalHolidayAllocationStatus.Archived;
            case SeasonalHolidayAllocationStatus.Resolved:
                switch (next)
                {
                    case SeasonalHolidayAllocationStatus.Reopened:
                    case SeasonalHolidayAllocationStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalHolidayAllocationStatus.Reopened:
                switch (next)
                {
                    case SeasonalHolidayAllocationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Ceiling;
    }

    public SeasonalHolidayAllocation Copy()
    {
        return new SeasonalHolidayAllocation(
            Code,
            Name,
            Notes,
            Archived,
            Description,
            Team,
            Ceiling,
            Location,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
