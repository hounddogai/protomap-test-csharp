namespace Acme.Workforce.Payrollops.Deduction.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction reservation is in its life cycle.</summary>
public enum DeductionReservationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One deduction reservation, with what the planners need to act on it.</summary>
public sealed class DeductionReservation(string code, string name, string channel, int quantity, double rate, string badge, int limit, double hours, DeductionReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int Quantity { get; set; } = quantity;
    public double Rate { get; set; } = rate;
    public string Badge { get; set; } = badge;
    public int Limit { get; set; } = limit;
    public double Hours { get; set; } = hours;
    public DeductionReservationStatus Status { get; set; } = status;

    /// <summary>Starts a deduction reservation in status idle.</summary>
    public static DeductionReservation NewDeductionReservation(string code, string name)
    {
        return new DeductionReservation(code, name, "", 0, 0.0, "", 0, 0.0, DeductionReservationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionReservationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionReservationStatus next)
    {
        switch (Status)
        {
            case DeductionReservationStatus.Idle:
                return next == DeductionReservationStatus.Warming || next == DeductionReservationStatus.Ready;
            case DeductionReservationStatus.Warming:
                return next == DeductionReservationStatus.Draining;
            case DeductionReservationStatus.Ready:
                switch (next)
                {
                    case DeductionReservationStatus.Draining:
                        return true;
                }
                return false;
            case DeductionReservationStatus.Draining:
                return next == DeductionReservationStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(DeductionReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"deduction reservation"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Limit * (double)Quantity, 2);
    }

    public DeductionReservation Copy()
    {
        return new DeductionReservation(Code, Name, Channel, Quantity, Rate, Badge, Limit, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
