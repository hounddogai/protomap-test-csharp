namespace Acme.Workforce.Absence.Holiday.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday mapping is in its life cycle.</summary>
public enum HolidayMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A holiday mapping. Its code is unique within a tenant.</summary>
public sealed class HolidayMapping(string code, string label, int createdAt, double margin, string department, double floorValue, string category, string location, bool visible, bool archived, int endsAt, HolidayMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public string Category { get; set; } = category;
    public string Location { get; set; } = location;
    public bool Visible { get; set; } = visible;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public HolidayMappingStatus Status { get; set; } = status;

    /// <summary>Starts a holiday mapping in status created.</summary>
    public static HolidayMapping NewHolidayMapping(string code, string label)
    {
        return new HolidayMapping(
            code,
            label,
            0,
            0.0,
            "",
            0.0,
            "",
            "",
            false,
            false,
            0,
            HolidayMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayMappingStatus next)
    {
        switch (Status)
        {
            case HolidayMappingStatus.Created:
                switch (next)
                {
                    case HolidayMappingStatus.Retired:
                        return true;
                }
                return false;
            case HolidayMappingStatus.Validated:
                return next == HolidayMappingStatus.Published || next == HolidayMappingStatus.Retired;
            case HolidayMappingStatus.Published:
                switch (next)
                {
                    case HolidayMappingStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Margin - (double)EndsAt, 2);
    }

    public HolidayMapping Copy()
    {
        return new HolidayMapping(
            Code,
            Label,
            CreatedAt,
            Margin,
            Department,
            FloorValue,
            Category,
            Location,
            Visible,
            Archived,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
