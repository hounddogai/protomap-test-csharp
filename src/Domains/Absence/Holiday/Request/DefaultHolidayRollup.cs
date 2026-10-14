namespace Acme.Workforce.Absence.Holiday.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default holiday rollup is in its life cycle.</summary>
public enum DefaultHolidayRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default holiday rollup. Its code is unique within a tenant.</summary>
public sealed class DefaultHolidayRollup(string code, string label, string summary, List<string> tags, int attempts, string phone, string grade, double cost, int capacity, string category, double discount, DefaultHolidayRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Summary { get; set; } = summary;
    public List<string> Tags { get; set; } = tags;
    public int Attempts { get; set; } = attempts;
    public string Phone { get; set; } = phone;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public int Capacity { get; set; } = capacity;
    public string Category { get; set; } = category;
    public double Discount { get; set; } = discount;
    public DefaultHolidayRollupStatus Status { get; set; } = status;

    /// <summary>Starts a default holiday rollup in status planned.</summary>
    public static DefaultHolidayRollup NewDefaultHolidayRollup(string code, string label)
    {
        return new DefaultHolidayRollup(
            code,
            label,
            "",
            new List<string>(),
            0,
            "",
            "",
            0.0,
            0,
            "",
            0.0,
            DefaultHolidayRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHolidayRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHolidayRollupStatus next)
    {
        switch (Status)
        {
            case DefaultHolidayRollupStatus.Planned:
                return next == DefaultHolidayRollupStatus.Completed || next == DefaultHolidayRollupStatus.Cancelled;
            case DefaultHolidayRollupStatus.Scheduled:
                switch (next)
                {
                    case DefaultHolidayRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultHolidayRollupStatus.InProgress:
                switch (next)
                {
                    case DefaultHolidayRollupStatus.Completed:
                        return true;
                }
                return false;
            case DefaultHolidayRollupStatus.Completed:
                return next == DefaultHolidayRollupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(DefaultHolidayRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default holiday rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double ProjectedExposure()
    {
        return Math.Round(Cost * (double)Capacity, 2);
    }

    public DefaultHolidayRollup Copy()
    {
        return new DefaultHolidayRollup(
            Code,
            Label,
            Summary,
            Tags,
            Attempts,
            Phone,
            Grade,
            Cost,
            Capacity,
            Category,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
