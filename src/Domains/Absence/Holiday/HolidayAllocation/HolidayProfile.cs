namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday profile is in its life cycle.</summary>
public enum HolidayProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A holiday profile. Its code is unique within a tenant.</summary>
public sealed class HolidayProfile(string code, string name, string source, double margin, List<string> tags, double floorValue, string locale, double threshold, double cost, int sequence, HolidayProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Source { get; set; } = source;
    public double Margin { get; set; } = margin;
    public List<string> Tags { get; set; } = tags;
    public double FloorValue { get; set; } = floorValue;
    public string Locale { get; set; } = locale;
    public double Threshold { get; set; } = threshold;
    public double Cost { get; set; } = cost;
    public int Sequence { get; set; } = sequence;
    public HolidayProfileStatus Status { get; set; } = status;

    /// <summary>Starts a holiday profile in status draft.</summary>
    public static HolidayProfile NewHolidayProfile(string code, string name)
    {
        return new HolidayProfile(
            code,
            name,
            "",
            0.0,
            new List<string>(),
            0.0,
            "",
            0.0,
            0.0,
            0,
            HolidayProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayProfileStatus next)
    {
        switch (Status)
        {
            case HolidayProfileStatus.Draft:
                switch (next)
                {
                    case HolidayProfileStatus.Pending:
                        return true;
                }
                return false;
            case HolidayProfileStatus.Pending:
                switch (next)
                {
                    case HolidayProfileStatus.Active:
                    case HolidayProfileStatus.Closed:
                        return true;
                }
                return false;
            case HolidayProfileStatus.Active:
                switch (next)
                {
                    case HolidayProfileStatus.Suspended:
                    case HolidayProfileStatus.Closed:
                        return true;
                }
                return false;
            case HolidayProfileStatus.Suspended:
                switch (next)
                {
                    case HolidayProfileStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)Sequence + FloorValue, 2);
    }

    public HolidayProfile Copy()
    {
        return new HolidayProfile(
            Code,
            Name,
            Source,
            Margin,
            Tags,
            FloorValue,
            Locale,
            Threshold,
            Cost,
            Sequence,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
