namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional timesheet profile is in its life cycle.</summary>
public enum RegionalTimesheetProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a regional timesheet profile and its current status.</summary>
public sealed class RegionalTimesheetProfile(string code, string name, double floorValue, string phone, int limit, string timezone, string grade, double margin, int batchSize, List<string> tags, RegionalTimesheetProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public int Limit { get; set; } = limit;
    public string Timezone { get; set; } = timezone;
    public string Grade { get; set; } = grade;
    public double Margin { get; set; } = margin;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public RegionalTimesheetProfileStatus Status { get; set; } = status;

    /// <summary>Starts a regional timesheet profile in status draft.</summary>
    public static RegionalTimesheetProfile NewRegionalTimesheetProfile(string code, string name)
    {
        return new RegionalTimesheetProfile(
            code,
            name,
            0.0,
            "",
            0,
            "",
            "",
            0.0,
            0,
            new List<string>(),
            RegionalTimesheetProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTimesheetProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTimesheetProfileStatus next)
    {
        switch (Status)
        {
            case RegionalTimesheetProfileStatus.Draft:
                return next == RegionalTimesheetProfileStatus.Pending;
            case RegionalTimesheetProfileStatus.Pending:
                return next == RegionalTimesheetProfileStatus.Active || next == RegionalTimesheetProfileStatus.Suspended;
            case RegionalTimesheetProfileStatus.Active:
                switch (next)
                {
                    case RegionalTimesheetProfileStatus.Closed:
                        return true;
                }
                return false;
            case RegionalTimesheetProfileStatus.Suspended:
                switch (next)
                {
                    case RegionalTimesheetProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalTimesheetProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional timesheet profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round(Margin + FloorValue, 2);
    }

    public RegionalTimesheetProfile Copy()
    {
        return new RegionalTimesheetProfile(
            Code,
            Name,
            FloorValue,
            Phone,
            Limit,
            Timezone,
            Grade,
            Margin,
            BatchSize,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
