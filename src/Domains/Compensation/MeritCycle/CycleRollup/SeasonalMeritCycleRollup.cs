namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal merit cycle rollup is in its life cycle.</summary>
public enum SeasonalMeritCycleRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal merit cycle rollup and its current status.</summary>
public sealed class SeasonalMeritCycleRollup(string code, string title, double weight, string grade, double ceiling, int endsAt, double ratio, int priority, SeasonalMeritCycleRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Weight { get; set; } = weight;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public int EndsAt { get; set; } = endsAt;
    public double Ratio { get; set; } = ratio;
    public int Priority { get; set; } = priority;
    public SeasonalMeritCycleRollupStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal merit cycle rollup in status proposed.</summary>
    public static SeasonalMeritCycleRollup NewSeasonalMeritCycleRollup(string code, string title)
    {
        return new SeasonalMeritCycleRollup(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            0,
            SeasonalMeritCycleRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalMeritCycleRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalMeritCycleRollupStatus next)
    {
        switch (Status)
        {
            case SeasonalMeritCycleRollupStatus.Proposed:
                switch (next)
                {
                    case SeasonalMeritCycleRollupStatus.Approved:
                    case SeasonalMeritCycleRollupStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalMeritCycleRollupStatus.Approved:
                switch (next)
                {
                    case SeasonalMeritCycleRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case SeasonalMeritCycleRollupStatus.Rejected:
                return next == SeasonalMeritCycleRollupStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalMeritCycleRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal merit cycle rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Weight + Ratio, 2);
    }

    public SeasonalMeritCycleRollup Copy()
    {
        return new SeasonalMeritCycleRollup(Code, Title, Weight, Grade, Ceiling, EndsAt, Ratio, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
