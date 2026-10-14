namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global nomination snapshot is in its life cycle.</summary>
public enum GlobalNominationSnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One global nomination snapshot, with what the planners need to act on it.</summary>
public sealed class GlobalNominationSnapshot(string code, string title, double rate, string name, string badge, GlobalNominationSnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public GlobalNominationSnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a global nomination snapshot in status new.</summary>
    public static GlobalNominationSnapshot NewGlobalNominationSnapshot(string code, string title)
    {
        return new GlobalNominationSnapshot(code, title, 0.0, "", "", GlobalNominationSnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalNominationSnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalNominationSnapshotStatus next)
    {
        switch (Status)
        {
            case GlobalNominationSnapshotStatus.New:
                return next == GlobalNominationSnapshotStatus.Queued;
            case GlobalNominationSnapshotStatus.Queued:
                return next == GlobalNominationSnapshotStatus.Done;
            case GlobalNominationSnapshotStatus.Running:
                return next == GlobalNominationSnapshotStatus.Done || next == GlobalNominationSnapshotStatus.Failed;
            case GlobalNominationSnapshotStatus.Done:
                switch (next)
                {
                    case GlobalNominationSnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalNominationSnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global nomination snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public GlobalNominationSnapshot Copy()
    {
        return new GlobalNominationSnapshot(Code, Title, Rate, Name, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
