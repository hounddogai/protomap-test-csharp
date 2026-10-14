namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling holiday mapping is in its life cycle.</summary>
public enum RollingHolidayMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling holiday mapping. Its code is unique within a tenant.</summary>
public sealed class RollingHolidayMapping(string code, string title, string employeeId, string summary, double threshold, List<string> tags, int priority, Dictionary<string, string> attributes, RollingHolidayMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string EmployeeId { get; set; } = employeeId;
    public string Summary { get; set; } = summary;
    public double Threshold { get; set; } = threshold;
    public List<string> Tags { get; set; } = tags;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public RollingHolidayMappingStatus Status { get; set; } = status;

    /// <summary>Starts a rolling holiday mapping in status open.</summary>
    public static RollingHolidayMapping NewRollingHolidayMapping(string code, string title)
    {
        return new RollingHolidayMapping(
            code,
            title,
            "",
            "",
            0.0,
            new List<string>(),
            0,
            new Dictionary<string, string>(),
            RollingHolidayMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingHolidayMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingHolidayMappingStatus next)
    {
        switch (Status)
        {
            case RollingHolidayMappingStatus.Open:
                return next == RollingHolidayMappingStatus.InReview || next == RollingHolidayMappingStatus.Archived;
            case RollingHolidayMappingStatus.InReview:
                return next == RollingHolidayMappingStatus.Reopened;
            case RollingHolidayMappingStatus.Resolved:
                switch (next)
                {
                    case RollingHolidayMappingStatus.Reopened:
                        return true;
                }
                return false;
            case RollingHolidayMappingStatus.Reopened:
                return next == RollingHolidayMappingStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingHolidayMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling holiday mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rolling holiday mapping"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)Priority * Threshold, 2);
    }

    public RollingHolidayMapping Copy()
    {
        return new RollingHolidayMapping(
            Code,
            Title,
            EmployeeId,
            Summary,
            Threshold,
            Tags,
            Priority,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
