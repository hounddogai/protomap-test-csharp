namespace Acme.Workforce.Compensation.MeritCycle.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional merit cycle bundle is in its life cycle.</summary>
public enum RegionalMeritCycleBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a regional merit cycle bundle and its current status.</summary>
public sealed class RegionalMeritCycleBundle(string code, string label, int graceDays, int revision, string team, double score, string ownerId, RegionalMeritCycleBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public int Revision { get; set; } = revision;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public string OwnerId { get; set; } = ownerId;
    public RegionalMeritCycleBundleStatus Status { get; set; } = status;

    /// <summary>Starts a regional merit cycle bundle in status new.</summary>
    public static RegionalMeritCycleBundle NewRegionalMeritCycleBundle(string code, string label)
    {
        return new RegionalMeritCycleBundle(code, label, 0, 0, "", 0.0, "", RegionalMeritCycleBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalMeritCycleBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalMeritCycleBundleStatus next)
    {
        switch (Status)
        {
            case RegionalMeritCycleBundleStatus.New:
                return next == RegionalMeritCycleBundleStatus.Queued;
            case RegionalMeritCycleBundleStatus.Queued:
                return next == RegionalMeritCycleBundleStatus.Running || next == RegionalMeritCycleBundleStatus.Done;
            case RegionalMeritCycleBundleStatus.Running:
                switch (next)
                {
                    case RegionalMeritCycleBundleStatus.Done:
                        return true;
                }
                return false;
            case RegionalMeritCycleBundleStatus.Done:
                return next == RegionalMeritCycleBundleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalMeritCycleBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional merit cycle bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"regional merit cycle bundle"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Score - (double)GraceDays, 2);
    }

    public RegionalMeritCycleBundle Copy()
    {
        return new RegionalMeritCycleBundle(Code, Label, GraceDays, Revision, Team, Score, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
