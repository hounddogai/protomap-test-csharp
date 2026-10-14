namespace Acme.Workforce.Contractors.Timesheet.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient timesheet preference is in its life cycle.</summary>
public enum LenientTimesheetPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A lenient timesheet preference as the timesheet team keeps it.</summary>
public sealed class LenientTimesheetPreference(string code, string title, double minimum, string grade, double ceiling, string country, int limit, int sequence, int revision, int durationMinutes, LenientTimesheetPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public string Country { get; set; } = country;
    public int Limit { get; set; } = limit;
    public int Sequence { get; set; } = sequence;
    public int Revision { get; set; } = revision;
    public int DurationMinutes { get; set; } = durationMinutes;
    public LenientTimesheetPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a lenient timesheet preference in status requested.</summary>
    public static LenientTimesheetPreference NewLenientTimesheetPreference(string code, string title)
    {
        return new LenientTimesheetPreference(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            0,
            0,
            0,
            0,
            LenientTimesheetPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientTimesheetPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientTimesheetPreferenceStatus next)
    {
        switch (Status)
        {
            case LenientTimesheetPreferenceStatus.Requested:
                switch (next)
                {
                    case LenientTimesheetPreferenceStatus.Revoked:
                        return true;
                }
                return false;
            case LenientTimesheetPreferenceStatus.Granted:
                switch (next)
                {
                    case LenientTimesheetPreferenceStatus.Expired:
                        return true;
                }
                return false;
            case LenientTimesheetPreferenceStatus.Revoked:
                switch (next)
                {
                    case LenientTimesheetPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientTimesheetPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient timesheet preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Minimum + (double)Sequence, 2);
    }

    public LenientTimesheetPreference Copy()
    {
        return new LenientTimesheetPreference(
            Code,
            Title,
            Minimum,
            Grade,
            Ceiling,
            Country,
            Limit,
            Sequence,
            Revision,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
