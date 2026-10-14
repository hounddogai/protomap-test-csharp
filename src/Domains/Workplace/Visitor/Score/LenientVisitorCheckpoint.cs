namespace Acme.Workforce.Workplace.Visitor.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient visitor checkpoint is in its life cycle.</summary>
public enum LenientVisitorCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One lenient visitor checkpoint, with what the planners need to act on it.</summary>
public sealed class LenientVisitorCheckpoint(string code, string name, double floorValue, bool archived, string label, int endsAt, int tenureMonths, LenientVisitorCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public bool Archived { get; set; } = archived;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public LenientVisitorCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a lenient visitor checkpoint in status idle.</summary>
    public static LenientVisitorCheckpoint NewLenientVisitorCheckpoint(string code, string name)
    {
        return new LenientVisitorCheckpoint(code, name, 0.0, false, "", 0, 0, LenientVisitorCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVisitorCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVisitorCheckpointStatus next)
    {
        switch (Status)
        {
            case LenientVisitorCheckpointStatus.Idle:
                return next == LenientVisitorCheckpointStatus.Draining || next == LenientVisitorCheckpointStatus.Stopped;
            case LenientVisitorCheckpointStatus.Warming:
                switch (next)
                {
                    case LenientVisitorCheckpointStatus.Ready:
                    case LenientVisitorCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case LenientVisitorCheckpointStatus.Ready:
                switch (next)
                {
                    case LenientVisitorCheckpointStatus.Stopped:
                        return true;
                }
                return false;
            case LenientVisitorCheckpointStatus.Draining:
                switch (next)
                {
                    case LenientVisitorCheckpointStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientVisitorCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient visitor checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient visitor checkpoint"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(FloorValue + (double)TenureMonths, 2);
    }

    public LenientVisitorCheckpoint Copy()
    {
        return new LenientVisitorCheckpoint(Code, Name, FloorValue, Archived, Label, EndsAt, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
