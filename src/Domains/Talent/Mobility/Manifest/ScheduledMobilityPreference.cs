namespace Acme.Workforce.Talent.Mobility.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility preference is in its life cycle.</summary>
public enum ScheduledMobilityPreferenceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One scheduled mobility preference, with what the planners need to act on it.</summary>
public sealed class ScheduledMobilityPreference(string code, string label, string jobCode, double hours, double floorValue, string salaryBand, bool archived, string category, int durationMinutes, ScheduledMobilityPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Hours { get; set; } = hours;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public string Category { get; set; } = category;
    public int DurationMinutes { get; set; } = durationMinutes;
    public ScheduledMobilityPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility preference in status draft.</summary>
    public static ScheduledMobilityPreference NewScheduledMobilityPreference(string code, string label)
    {
        return new ScheduledMobilityPreference(
            code,
            label,
            "",
            0.0,
            0.0,
            "",
            false,
            "",
            0,
            ScheduledMobilityPreferenceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityPreferenceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityPreferenceStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityPreferenceStatus.Draft:
                switch (next)
                {
                    case ScheduledMobilityPreferenceStatus.Active:
                    case ScheduledMobilityPreferenceStatus.Suspended:
                        return true;
                }
                return false;
            case ScheduledMobilityPreferenceStatus.Pending:
                switch (next)
                {
                    case ScheduledMobilityPreferenceStatus.Suspended:
                    case ScheduledMobilityPreferenceStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledMobilityPreferenceStatus.Active:
                return next == ScheduledMobilityPreferenceStatus.Suspended || next == ScheduledMobilityPreferenceStatus.Closed;
            case ScheduledMobilityPreferenceStatus.Suspended:
                switch (next)
                {
                    case ScheduledMobilityPreferenceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled mobility preference"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)DurationMinutes + FloorValue, 2);
    }

    public ScheduledMobilityPreference Copy()
    {
        return new ScheduledMobilityPreference(
            Code,
            Label,
            JobCode,
            Hours,
            FloorValue,
            SalaryBand,
            Archived,
            Category,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
