namespace Acme.Workforce.Engagement.TownHall.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall schedule is in its life cycle.</summary>
public enum LegacyTownHallScheduleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy town hall schedule as the town hall team keeps it.</summary>
public sealed class LegacyTownHallSchedule(string code, string title, double ceiling, int batchSize, bool pinned, int revision, string currency, string source, int capacity, double threshold, double amount, LegacyTownHallScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public int BatchSize { get; set; } = batchSize;
    public bool Pinned { get; set; } = pinned;
    public int Revision { get; set; } = revision;
    public string Currency { get; set; } = currency;
    public string Source { get; set; } = source;
    public int Capacity { get; set; } = capacity;
    public double Threshold { get; set; } = threshold;
    public double Amount { get; set; } = amount;
    public LegacyTownHallScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall schedule in status open.</summary>
    public static LegacyTownHallSchedule NewLegacyTownHallSchedule(string code, string title)
    {
        return new LegacyTownHallSchedule(
            code,
            title,
            0.0,
            0,
            false,
            0,
            "",
            "",
            0,
            0.0,
            0.0,
            LegacyTownHallScheduleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallScheduleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallScheduleStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallScheduleStatus.Open:
                switch (next)
                {
                    case LegacyTownHallScheduleStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyTownHallScheduleStatus.InReview:
                return next == LegacyTownHallScheduleStatus.Reopened || next == LegacyTownHallScheduleStatus.Archived;
            case LegacyTownHallScheduleStatus.Resolved:
                switch (next)
                {
                    case LegacyTownHallScheduleStatus.Reopened:
                    case LegacyTownHallScheduleStatus.Archived:
                        return true;
                }
                return false;
            case LegacyTownHallScheduleStatus.Reopened:
                switch (next)
                {
                    case LegacyTownHallScheduleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)BatchSize - (double)Capacity, 2);
    }

    public LegacyTownHallSchedule Copy()
    {
        return new LegacyTownHallSchedule(
            Code,
            Title,
            Ceiling,
            BatchSize,
            Pinned,
            Revision,
            Currency,
            Source,
            Capacity,
            Threshold,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
