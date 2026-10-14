namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool reservation is in its life cycle.</summary>
public enum PoolReservationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pool reservation, with what the planners need to act on it.</summary>
public sealed class PoolReservation(string code, string label, int capacity, double score, double floorValue, PoolReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Capacity { get; set; } = capacity;
    public double Score { get; set; } = score;
    public double FloorValue { get; set; } = floorValue;
    public PoolReservationStatus Status { get; set; } = status;

    /// <summary>Starts a pool reservation in status planned.</summary>
    public static PoolReservation NewPoolReservation(string code, string label)
    {
        return new PoolReservation(code, label, 0, 0.0, 0.0, PoolReservationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolReservationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolReservationStatus next)
    {
        switch (Status)
        {
            case PoolReservationStatus.Planned:
                switch (next)
                {
                    case PoolReservationStatus.InProgress:
                        return true;
                }
                return false;
            case PoolReservationStatus.Scheduled:
                return next == PoolReservationStatus.Cancelled;
            case PoolReservationStatus.InProgress:
                return next == PoolReservationStatus.Completed || next == PoolReservationStatus.Cancelled;
            case PoolReservationStatus.Completed:
                return next == PoolReservationStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PoolReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)Capacity * Score, 2);
    }

    public PoolReservation Copy()
    {
        return new PoolReservation(Code, Label, Capacity, Score, FloorValue, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
