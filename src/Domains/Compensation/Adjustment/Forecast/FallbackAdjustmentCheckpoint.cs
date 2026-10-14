namespace Acme.Workforce.Compensation.Adjustment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback adjustment checkpoint is in its life cycle.</summary>
public enum FallbackAdjustmentCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback adjustment checkpoint and its current status.</summary>
public sealed class FallbackAdjustmentCheckpoint(string code, string name, int limit, int revision, bool active, FallbackAdjustmentCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public int Revision { get; set; } = revision;
    public bool Active { get; set; } = active;
    public FallbackAdjustmentCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a fallback adjustment checkpoint in status idle.</summary>
    public static FallbackAdjustmentCheckpoint NewFallbackAdjustmentCheckpoint(string code, string name)
    {
        return new FallbackAdjustmentCheckpoint(code, name, 0, 0, false, FallbackAdjustmentCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAdjustmentCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAdjustmentCheckpointStatus next)
    {
        switch (Status)
        {
            case FallbackAdjustmentCheckpointStatus.Idle:
                return next == FallbackAdjustmentCheckpointStatus.Warming || next == FallbackAdjustmentCheckpointStatus.Ready;
            case FallbackAdjustmentCheckpointStatus.Warming:
                return next == FallbackAdjustmentCheckpointStatus.Ready || next == FallbackAdjustmentCheckpointStatus.Stopped;
            case FallbackAdjustmentCheckpointStatus.Ready:
                return next == FallbackAdjustmentCheckpointStatus.Stopped;
            case FallbackAdjustmentCheckpointStatus.Draining:
                switch (next)
                {
                    case FallbackAdjustmentCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAdjustmentCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback adjustment checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit * (double)Revision, 2);
    }

    public FallbackAdjustmentCheckpoint Copy()
    {
        return new FallbackAdjustmentCheckpoint(Code, Name, Limit, Revision, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
