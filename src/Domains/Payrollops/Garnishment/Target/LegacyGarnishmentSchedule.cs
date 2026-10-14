namespace Acme.Workforce.Payrollops.Garnishment.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy garnishment schedule is in its life cycle.</summary>
public enum LegacyGarnishmentScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a legacy garnishment schedule and its current status.</summary>
public sealed class LegacyGarnishmentSchedule(string code, string name, int leaveDays, List<string> tags, int retries, double weight, double rating, string locale, LegacyGarnishmentScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public int Retries { get; set; } = retries;
    public double Weight { get; set; } = weight;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public LegacyGarnishmentScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy garnishment schedule in status proposed.</summary>
    public static LegacyGarnishmentSchedule NewLegacyGarnishmentSchedule(string code, string name)
    {
        return new LegacyGarnishmentSchedule(
            code,
            name,
            0,
            new List<string>(),
            0,
            0.0,
            0.0,
            "",
            LegacyGarnishmentScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyGarnishmentScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyGarnishmentScheduleStatus next)
    {
        switch (Status)
        {
            case LegacyGarnishmentScheduleStatus.Proposed:
                return next == LegacyGarnishmentScheduleStatus.Rejected || next == LegacyGarnishmentScheduleStatus.Withdrawn;
            case LegacyGarnishmentScheduleStatus.Approved:
                switch (next)
                {
                    case LegacyGarnishmentScheduleStatus.Rejected:
                    case LegacyGarnishmentScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case LegacyGarnishmentScheduleStatus.Rejected:
                switch (next)
                {
                    case LegacyGarnishmentScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyGarnishmentScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy garnishment schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"legacy garnishment schedule"} {Code}";
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

    public double WeightedTotal()
    {
        return Math.Round((double)Retries + (double)LeaveDays, 2);
    }

    public LegacyGarnishmentSchedule Copy()
    {
        return new LegacyGarnishmentSchedule(Code, Name, LeaveDays, Tags, Retries, Weight, Rating, Locale, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
