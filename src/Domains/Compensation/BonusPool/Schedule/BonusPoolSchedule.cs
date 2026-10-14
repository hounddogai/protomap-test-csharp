namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool schedule is in its life cycle.</summary>
public enum BonusPoolScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One bonus pool schedule, with what the planners need to act on it.</summary>
public sealed class BonusPoolSchedule(string code, string title, string locale, double amount, int limit, List<string> tags, BonusPoolScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double Amount { get; set; } = amount;
    public int Limit { get; set; } = limit;
    public List<string> Tags { get; set; } = tags;
    public BonusPoolScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool schedule in status proposed.</summary>
    public static BonusPoolSchedule NewBonusPoolSchedule(string code, string title)
    {
        return new BonusPoolSchedule(code, title, "", 0.0, 0, new List<string>(), BonusPoolScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolScheduleStatus next)
    {
        switch (Status)
        {
            case BonusPoolScheduleStatus.Proposed:
                switch (next)
                {
                    case BonusPoolScheduleStatus.Approved:
                        return true;
                }
                return false;
            case BonusPoolScheduleStatus.Approved:
                switch (next)
                {
                    case BonusPoolScheduleStatus.Rejected:
                        return true;
                }
                return false;
            case BonusPoolScheduleStatus.Rejected:
                return next == BonusPoolScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BonusPoolScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double WeightedExposure()
    {
        return Math.Round(Amount * (double)Limit, 2);
    }

    public BonusPoolSchedule Copy()
    {
        return new BonusPoolSchedule(Code, Title, Locale, Amount, Limit, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
