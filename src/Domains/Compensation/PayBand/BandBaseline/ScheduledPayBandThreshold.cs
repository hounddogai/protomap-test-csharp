namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay band threshold is in its life cycle.</summary>
public enum ScheduledPayBandThresholdStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scheduled pay band threshold, with what the planners need to act on it.</summary>
public sealed class ScheduledPayBandThreshold(string code, string title, double hours, string managerId, bool pinned, bool approved, string jobCode, ScheduledPayBandThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public bool Pinned { get; set; } = pinned;
    public bool Approved { get; set; } = approved;
    public string JobCode { get; set; } = jobCode;
    public ScheduledPayBandThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay band threshold in status idle.</summary>
    public static ScheduledPayBandThreshold NewScheduledPayBandThreshold(string code, string title)
    {
        return new ScheduledPayBandThreshold(
            code,
            title,
            0.0,
            "",
            false,
            false,
            "",
            ScheduledPayBandThresholdStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayBandThresholdStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayBandThresholdStatus next)
    {
        switch (Status)
        {
            case ScheduledPayBandThresholdStatus.Idle:
                switch (next)
                {
                    case ScheduledPayBandThresholdStatus.Ready:
                    case ScheduledPayBandThresholdStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledPayBandThresholdStatus.Warming:
                return next == ScheduledPayBandThresholdStatus.Draining || next == ScheduledPayBandThresholdStatus.Stopped;
            case ScheduledPayBandThresholdStatus.Ready:
                return next == ScheduledPayBandThresholdStatus.Draining || next == ScheduledPayBandThresholdStatus.Stopped;
            case ScheduledPayBandThresholdStatus.Draining:
                return next == ScheduledPayBandThresholdStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledPayBandThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay band threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return Hours;
    }

    public ScheduledPayBandThreshold Copy()
    {
        return new ScheduledPayBandThreshold(Code, Title, Hours, ManagerId, Pinned, Approved, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
