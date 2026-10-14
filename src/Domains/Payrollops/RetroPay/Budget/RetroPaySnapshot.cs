namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay snapshot is in its life cycle.</summary>
public enum RetroPaySnapshotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A retro pay snapshot. Its code is unique within a tenant.</summary>
public sealed class RetroPaySnapshot(string code, string label, double ceiling, string summary, string location, int limit, double score, string segment, string source, RetroPaySnapshotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Ceiling { get; set; } = ceiling;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public RetroPaySnapshotStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay snapshot in status new.</summary>
    public static RetroPaySnapshot NewRetroPaySnapshot(string code, string label)
    {
        return new RetroPaySnapshot(code, label, 0.0, "", "", 0, 0.0, "", "", RetroPaySnapshotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPaySnapshotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPaySnapshotStatus next)
    {
        switch (Status)
        {
            case RetroPaySnapshotStatus.New:
                return next == RetroPaySnapshotStatus.Done;
            case RetroPaySnapshotStatus.Queued:
                return next == RetroPaySnapshotStatus.Running || next == RetroPaySnapshotStatus.Failed;
            case RetroPaySnapshotStatus.Running:
                return next == RetroPaySnapshotStatus.Done || next == RetroPaySnapshotStatus.Failed;
            case RetroPaySnapshotStatus.Done:
                switch (next)
                {
                    case RetroPaySnapshotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPaySnapshotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay snapshot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedVolume()
    {
        return Math.Round(Ceiling + Score, 2);
    }

    public RetroPaySnapshot Copy()
    {
        return new RetroPaySnapshot(Code, Label, Ceiling, Summary, Location, Limit, Score, Segment, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
