namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary merit cycle rollup is in its life cycle.</summary>
public enum PrimaryMeritCycleRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One primary merit cycle rollup, with what the planners need to act on it.</summary>
public sealed class PrimaryMeritCycleRollup(string code, string title, string currency, bool archived, string skill, int expiresAt, int graceDays, string summary, string label, PrimaryMeritCycleRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public bool Archived { get; set; } = archived;
    public string Skill { get; set; } = skill;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public string Summary { get; set; } = summary;
    public string Label { get; set; } = label;
    public PrimaryMeritCycleRollupStatus Status { get; set; } = status;

    /// <summary>Starts a primary merit cycle rollup in status new.</summary>
    public static PrimaryMeritCycleRollup NewPrimaryMeritCycleRollup(string code, string title)
    {
        return new PrimaryMeritCycleRollup(
            code,
            title,
            "",
            false,
            "",
            0,
            0,
            "",
            "",
            PrimaryMeritCycleRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMeritCycleRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMeritCycleRollupStatus next)
    {
        switch (Status)
        {
            case PrimaryMeritCycleRollupStatus.New:
                return next == PrimaryMeritCycleRollupStatus.Failed;
            case PrimaryMeritCycleRollupStatus.Queued:
                return next == PrimaryMeritCycleRollupStatus.Done;
            case PrimaryMeritCycleRollupStatus.Running:
                switch (next)
                {
                    case PrimaryMeritCycleRollupStatus.Done:
                    case PrimaryMeritCycleRollupStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryMeritCycleRollupStatus.Done:
                switch (next)
                {
                    case PrimaryMeritCycleRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryMeritCycleRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary merit cycle rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"primary merit cycle rollup"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)ExpiresAt + (double)GraceDays, 2);
    }

    public PrimaryMeritCycleRollup Copy()
    {
        return new PrimaryMeritCycleRollup(
            Code,
            Title,
            Currency,
            Archived,
            Skill,
            ExpiresAt,
            GraceDays,
            Summary,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
