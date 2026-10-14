namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal town hall group is in its life cycle.</summary>
public enum SeasonalTownHallGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A seasonal town hall group. Its code is unique within a tenant.</summary>
public sealed class SeasonalTownHallGroup(string code, string title, string reviewer, double maximum, int updatedAt, double floorValue, string notes, string locale, SeasonalTownHallGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Reviewer { get; set; } = reviewer;
    public double Maximum { get; set; } = maximum;
    public int UpdatedAt { get; set; } = updatedAt;
    public double FloorValue { get; set; } = floorValue;
    public string Notes { get; set; } = notes;
    public string Locale { get; set; } = locale;
    public SeasonalTownHallGroupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal town hall group in status new.</summary>
    public static SeasonalTownHallGroup NewSeasonalTownHallGroup(string code, string title)
    {
        return new SeasonalTownHallGroup(code, title, "", 0.0, 0, 0.0, "", "", SeasonalTownHallGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalTownHallGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalTownHallGroupStatus next)
    {
        switch (Status)
        {
            case SeasonalTownHallGroupStatus.New:
                return next == SeasonalTownHallGroupStatus.Queued || next == SeasonalTownHallGroupStatus.Done;
            case SeasonalTownHallGroupStatus.Queued:
                switch (next)
                {
                    case SeasonalTownHallGroupStatus.Done:
                        return true;
                }
                return false;
            case SeasonalTownHallGroupStatus.Running:
                switch (next)
                {
                    case SeasonalTownHallGroupStatus.Done:
                    case SeasonalTownHallGroupStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalTownHallGroupStatus.Done:
                return next == SeasonalTownHallGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalTownHallGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal town hall group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)UpdatedAt - FloorValue, 2);
    }

    public SeasonalTownHallGroup Copy()
    {
        return new SeasonalTownHallGroup(
            Code,
            Title,
            Reviewer,
            Maximum,
            UpdatedAt,
            FloorValue,
            Notes,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
