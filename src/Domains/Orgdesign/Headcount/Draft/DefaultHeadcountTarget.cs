namespace Acme.Workforce.Orgdesign.Headcount.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default headcount target is in its life cycle.</summary>
public enum DefaultHeadcountTargetStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A default headcount target. Its code is unique within a tenant.</summary>
public sealed class DefaultHeadcountTarget(string code, string name, int durationMinutes, double threshold, string timezone, string salaryBand, string category, string channel, double weight, int windowDays, DefaultHeadcountTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Threshold { get; set; } = threshold;
    public string Timezone { get; set; } = timezone;
    public string SalaryBand { get; set; } = salaryBand;
    public string Category { get; set; } = category;
    public string Channel { get; set; } = channel;
    public double Weight { get; set; } = weight;
    public int WindowDays { get; set; } = windowDays;
    public DefaultHeadcountTargetStatus Status { get; set; } = status;

    /// <summary>Starts a default headcount target in status proposed.</summary>
    public static DefaultHeadcountTarget NewDefaultHeadcountTarget(string code, string name)
    {
        return new DefaultHeadcountTarget(
            code,
            name,
            0,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            0,
            DefaultHeadcountTargetStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultHeadcountTargetStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultHeadcountTargetStatus next)
    {
        switch (Status)
        {
            case DefaultHeadcountTargetStatus.Proposed:
                switch (next)
                {
                    case DefaultHeadcountTargetStatus.Approved:
                    case DefaultHeadcountTargetStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultHeadcountTargetStatus.Approved:
                return next == DefaultHeadcountTargetStatus.Rejected || next == DefaultHeadcountTargetStatus.Withdrawn;
            case DefaultHeadcountTargetStatus.Rejected:
                switch (next)
                {
                    case DefaultHeadcountTargetStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultHeadcountTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default headcount target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"default headcount target"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Threshold - (double)DurationMinutes, 2);
    }

    public DefaultHeadcountTarget Copy()
    {
        return new DefaultHeadcountTarget(
            Code,
            Name,
            DurationMinutes,
            Threshold,
            Timezone,
            SalaryBand,
            Category,
            Channel,
            Weight,
            WindowDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
