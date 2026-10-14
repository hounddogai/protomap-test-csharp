namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped career path schedule is in its life cycle.</summary>
public enum CappedCareerPathScheduleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One capped career path schedule, with what the planners need to act on it.</summary>
public sealed class CappedCareerPathSchedule(string code, string label, int windowDays, string locale, string currency, string name, string timezone, double score, int sequence, string country, string reviewer, CappedCareerPathScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public double Score { get; set; } = score;
    public int Sequence { get; set; } = sequence;
    public string Country { get; set; } = country;
    public string Reviewer { get; set; } = reviewer;
    public CappedCareerPathScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a capped career path schedule in status planned.</summary>
    public static CappedCareerPathSchedule NewCappedCareerPathSchedule(string code, string label)
    {
        return new CappedCareerPathSchedule(
            code,
            label,
            0,
            "",
            "",
            "",
            "",
            0.0,
            0,
            "",
            "",
            CappedCareerPathScheduleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedCareerPathScheduleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedCareerPathScheduleStatus next)
    {
        switch (Status)
        {
            case CappedCareerPathScheduleStatus.Planned:
                switch (next)
                {
                    case CappedCareerPathScheduleStatus.Completed:
                        return true;
                }
                return false;
            case CappedCareerPathScheduleStatus.Scheduled:
                switch (next)
                {
                    case CappedCareerPathScheduleStatus.InProgress:
                    case CappedCareerPathScheduleStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedCareerPathScheduleStatus.InProgress:
                return next == CappedCareerPathScheduleStatus.Cancelled;
            case CappedCareerPathScheduleStatus.Completed:
                switch (next)
                {
                    case CappedCareerPathScheduleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedCareerPathScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped career path schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Sequence * Score, 2);
    }

    public CappedCareerPathSchedule Copy()
    {
        return new CappedCareerPathSchedule(
            Code,
            Label,
            WindowDays,
            Locale,
            Currency,
            Name,
            Timezone,
            Score,
            Sequence,
            Country,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
