namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive reorg snapshot is in its life cycle.</summary>
public enum AdaptiveReorgSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive reorg snapshot, with what the planners need to act on it.</summary>
public sealed class AdaptiveReorgSnapshot(string code, string title, string label, double ratio, double budget, string reviewer, string team, AdaptiveReorgSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Label { get; set; } = label;
    public double Ratio { get; set; } = ratio;
    public double Budget { get; set; } = budget;
    public string Reviewer { get; set; } = reviewer;
    public string Team { get; set; } = team;
    public AdaptiveReorgSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive reorg snapshot in status new.</summary>
    public static AdaptiveReorgSnapshot NewAdaptiveReorgSnapshot(string code, string title)
    {
        return new AdaptiveReorgSnapshot(code, title, "", 0.0, 0.0, "", "", AdaptiveReorgSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveReorgSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveReorgSnapshotStatus next)
    {
        switch (Status)
        {
            case AdaptiveReorgSnapshotStatus.New:
                return next == AdaptiveReorgSnapshotStatus.Running;
            case AdaptiveReorgSnapshotStatus.Queued:
                switch (next)
                {
                    case AdaptiveReorgSnapshotStatus.Done:
                    case AdaptiveReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveReorgSnapshotStatus.Running:
                switch (next)
                {
                    case AdaptiveReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveReorgSnapshotStatus.Done:
                switch (next)
                {
                    case AdaptiveReorgSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveReorgSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive reorg snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive reorg snapshot"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Ratio - Budget, 2);
    }

    public AdaptiveReorgSnapshot Copy()
    {
        return new AdaptiveReorgSnapshot(Code, Title, Label, Ratio, Budget, Reviewer, Team, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
