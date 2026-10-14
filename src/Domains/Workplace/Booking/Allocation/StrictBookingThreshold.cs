namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict booking threshold is in its life cycle.</summary>
public enum StrictBookingThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict booking threshold and its current status.</summary>
public sealed class StrictBookingThreshold(string code, string name, string channel, double maximum, double threshold, int limit, StrictBookingThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public double Maximum { get; set; } = maximum;
    public double Threshold { get; set; } = threshold;
    public int Limit { get; set; } = limit;
    public StrictBookingThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a strict booking threshold in status idle.</summary>
    public static StrictBookingThreshold NewStrictBookingThreshold(string code, string name)
    {
        return new StrictBookingThreshold(code, name, "", 0.0, 0.0, 0, StrictBookingThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBookingThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBookingThresholdStatus next)
    {
        switch (Status)
        {
            case StrictBookingThresholdStatus.Idle:
                switch (next)
                {
                    case StrictBookingThresholdStatus.Warming:
                    case StrictBookingThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case StrictBookingThresholdStatus.Warming:
                switch (next)
                {
                    case StrictBookingThresholdStatus.Draining:
                        return true;
                }
                return false;
            case StrictBookingThresholdStatus.Ready:
                return next == StrictBookingThresholdStatus.Stopped;
            case StrictBookingThresholdStatus.Draining:
                return next == StrictBookingThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictBookingThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict booking threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Limit - Threshold, 2);
    }

    public StrictBookingThreshold Copy()
    {
        return new StrictBookingThreshold(Code, Name, Channel, Maximum, Threshold, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
