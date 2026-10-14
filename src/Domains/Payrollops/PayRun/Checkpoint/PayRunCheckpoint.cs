namespace Acme.Workforce.Payrollops.PayRun.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run checkpoint is in its life cycle.</summary>
public enum PayRunCheckpointStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pay run checkpoint, with what the planners need to act on it.</summary>
public sealed class PayRunCheckpoint(string code, string label, string team, double floorValue, string phone, double margin, int batchSize, double weight, string department, int graceDays, PayRunCheckpointStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public double Margin { get; set; } = margin;
    public int BatchSize { get; set; } = batchSize;
    public double Weight { get; set; } = weight;
    public string Department { get; set; } = department;
    public int GraceDays { get; set; } = graceDays;
    public PayRunCheckpointStatus Status { get; set; } = status;

    /// <summary>Starts a pay run checkpoint in status idle.</summary>
    public static PayRunCheckpoint NewPayRunCheckpoint(string code, string label)
    {
        return new PayRunCheckpoint(code, label, "", 0.0, "", 0.0, 0, 0.0, "", 0, PayRunCheckpointStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunCheckpointStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunCheckpointStatus next)
    {
        switch (Status)
        {
            case PayRunCheckpointStatus.Idle:
                return next == PayRunCheckpointStatus.Warming || next == PayRunCheckpointStatus.Ready;
            case PayRunCheckpointStatus.Warming:
                return next == PayRunCheckpointStatus.Draining;
            case PayRunCheckpointStatus.Ready:
                switch (next)
                {
                    case PayRunCheckpointStatus.Draining:
                        return true;
                }
                return false;
            case PayRunCheckpointStatus.Draining:
                return next == PayRunCheckpointStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PayRunCheckpointStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run checkpoint {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pay run checkpoint"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(FloorValue + Margin, 2);
    }

    public PayRunCheckpoint Copy()
    {
        return new PayRunCheckpoint(
            Code,
            Label,
            Team,
            FloorValue,
            Phone,
            Margin,
            BatchSize,
            Weight,
            Department,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
