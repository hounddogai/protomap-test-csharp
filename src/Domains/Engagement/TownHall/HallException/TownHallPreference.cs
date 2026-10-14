namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall preference is in its life cycle.</summary>
public enum TownHallPreferenceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A town hall preference. Its code is unique within a tenant.</summary>
public sealed class TownHallPreference(string code, string label, int startsAt, double hours, double rate, double score, double maximum, bool approved, int durationMinutes, int updatedAt, string name, TownHallPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Hours { get; set; } = hours;
    public double Rate { get; set; } = rate;
    public double Score { get; set; } = score;
    public double Maximum { get; set; } = maximum;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Name { get; set; } = name;
    public TownHallPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a town hall preference in status new.</summary>
    public static TownHallPreference NewTownHallPreference(string code, string label)
    {
        return new TownHallPreference(
            code,
            label,
            0,
            0.0,
            0.0,
            0.0,
            0.0,
            false,
            0,
            0,
            "",
            TownHallPreferenceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallPreferenceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallPreferenceStatus next)
    {
        switch (Status)
        {
            case TownHallPreferenceStatus.New:
                return next == TownHallPreferenceStatus.Queued || next == TownHallPreferenceStatus.Failed;
            case TownHallPreferenceStatus.Queued:
                switch (next)
                {
                    case TownHallPreferenceStatus.Running:
                    case TownHallPreferenceStatus.Done:
                        return true;
                }
                return false;
            case TownHallPreferenceStatus.Running:
                return next == TownHallPreferenceStatus.Done || next == TownHallPreferenceStatus.Failed;
            case TownHallPreferenceStatus.Done:
                return next == TownHallPreferenceStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TownHallPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"town hall preference"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)StartsAt - Rate, 2);
    }

    public TownHallPreference Copy()
    {
        return new TownHallPreference(
            Code,
            Label,
            StartsAt,
            Hours,
            Rate,
            Score,
            Maximum,
            Approved,
            DurationMinutes,
            UpdatedAt,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
