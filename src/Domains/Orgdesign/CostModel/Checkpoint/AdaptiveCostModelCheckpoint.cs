namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive cost model checkpoint is in its life cycle.</summary>
public enum AdaptiveCostModelCheckpointStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One adaptive cost model checkpoint, with what the planners need to act on it.</summary>
public sealed class AdaptiveCostModelCheckpoint(string code, string name, bool archived, int windowDays, int graceDays, double minimum, string unit, AdaptiveCostModelCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Archived { get; set; } = archived;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public double Minimum { get; set; } = minimum;
    public string Unit { get; set; } = unit;
    public AdaptiveCostModelCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive cost model checkpoint in status new.</summary>
    public static AdaptiveCostModelCheckpoint NewAdaptiveCostModelCheckpoint(string code, string name)
    {
        return new AdaptiveCostModelCheckpoint(
            code,
            name,
            false,
            0,
            0,
            0.0,
            "",
            AdaptiveCostModelCheckpointStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveCostModelCheckpointStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveCostModelCheckpointStatus next)
    {
        switch (Status)
        {
            case AdaptiveCostModelCheckpointStatus.New:
                switch (next)
                {
                    case AdaptiveCostModelCheckpointStatus.Queued:
                        return true;
                }
                return false;
            case AdaptiveCostModelCheckpointStatus.Queued:
                return next == AdaptiveCostModelCheckpointStatus.Running || next == AdaptiveCostModelCheckpointStatus.Done;
            case AdaptiveCostModelCheckpointStatus.Running:
                return next == AdaptiveCostModelCheckpointStatus.Done;
            case AdaptiveCostModelCheckpointStatus.Done:
                return next == AdaptiveCostModelCheckpointStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AdaptiveCostModelCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive cost model checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)WindowDays * Minimum, 2);
    }

    public AdaptiveCostModelCheckpoint Copy()
    {
        return new AdaptiveCostModelCheckpoint(Code, Name, Archived, WindowDays, GraceDays, Minimum, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
