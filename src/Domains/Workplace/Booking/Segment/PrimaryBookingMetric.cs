namespace Acme.Workforce.Workplace.Booking.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary booking metric is in its life cycle.</summary>
public enum PrimaryBookingMetricStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary booking metric as the booking team keeps it.</summary>
public sealed class PrimaryBookingMetric(string code, string name, double ratio, double hours, bool active, string contactEmail, string phone, int priority, string externalRef, string timezone, int batchSize, PrimaryBookingMetricStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public double Hours { get; set; } = hours;
    public bool Active { get; set; } = active;
    public string ContactEmail { get; set; } = contactEmail;
    public string Phone { get; set; } = phone;
    public int Priority { get; set; } = priority;
    public string ExternalRef { get; set; } = externalRef;
    public string Timezone { get; set; } = timezone;
    public int BatchSize { get; set; } = batchSize;
    public PrimaryBookingMetricStatus Status { get; set; } = status;

    /// <summary>Starts a primary booking metric in status idle.</summary>
    public static PrimaryBookingMetric NewPrimaryBookingMetric(string code, string name)
    {
        return new PrimaryBookingMetric(
            code,
            name,
            0.0,
            0.0,
            false,
            "",
            "",
            0,
            "",
            "",
            0,
            PrimaryBookingMetricStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryBookingMetricStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryBookingMetricStatus next)
    {
        switch (Status)
        {
            case PrimaryBookingMetricStatus.Idle:
                return next == PrimaryBookingMetricStatus.Ready;
            case PrimaryBookingMetricStatus.Warming:
                return next == PrimaryBookingMetricStatus.Ready || next == PrimaryBookingMetricStatus.Stopped;
            case PrimaryBookingMetricStatus.Ready:
                switch (next)
                {
                    case PrimaryBookingMetricStatus.Draining:
                        return true;
                }
                return false;
            case PrimaryBookingMetricStatus.Draining:
                switch (next)
                {
                    case PrimaryBookingMetricStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryBookingMetricStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary booking metric {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Hours - Ratio, 2);
    }

    public PrimaryBookingMetric Copy()
    {
        return new PrimaryBookingMetric(
            Code,
            Name,
            Ratio,
            Hours,
            Active,
            ContactEmail,
            Phone,
            Priority,
            ExternalRef,
            Timezone,
            BatchSize,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
