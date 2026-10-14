namespace Acme.Workforce.Talent.CareerPath.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path mapping is in its life cycle.</summary>
public enum CareerPathMappingStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One career path mapping, with what the planners need to act on it.</summary>
public sealed class CareerPathMapping(string code, string label, int leaveDays, string notes, Dictionary<string, string> attributes, string checksum, string country, int priority, string location, int durationMinutes, int createdAt, CareerPathMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public string Notes { get; set; } = notes;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Checksum { get; set; } = checksum;
    public string Country { get; set; } = country;
    public int Priority { get; set; } = priority;
    public string Location { get; set; } = location;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int CreatedAt { get; set; } = createdAt;
    public CareerPathMappingStatus Status { get; set; } = status;

    /// <summary>Starts a career path mapping in status planned.</summary>
    public static CareerPathMapping NewCareerPathMapping(string code, string label)
    {
        return new CareerPathMapping(
            code,
            label,
            0,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            0,
            0,
            CareerPathMappingStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathMappingStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathMappingStatus next)
    {
        switch (Status)
        {
            case CareerPathMappingStatus.Planned:
                return next == CareerPathMappingStatus.Cancelled;
            case CareerPathMappingStatus.Scheduled:
                return next == CareerPathMappingStatus.InProgress;
            case CareerPathMappingStatus.InProgress:
                return next == CareerPathMappingStatus.Completed || next == CareerPathMappingStatus.Cancelled;
            case CareerPathMappingStatus.Completed:
                switch (next)
                {
                    case CareerPathMappingStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)LeaveDays - (double)CreatedAt, 2);
    }

    public CareerPathMapping Copy()
    {
        return new CareerPathMapping(
            Code,
            Label,
            LeaveDays,
            Notes,
            Attributes,
            Checksum,
            Country,
            Priority,
            Location,
            DurationMinutes,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
