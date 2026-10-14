namespace Acme.Workforce.Payrollops.PayRun.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pay run request is in its life cycle.</summary>
public enum ScheduledPayRunRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled pay run request, with what the planners need to act on it.</summary>
public sealed class ScheduledPayRunRequest(string code, string name, string region, int windowDays, string timezone, ScheduledPayRunRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public ScheduledPayRunRequestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pay run request in status new.</summary>
    public static ScheduledPayRunRequest NewScheduledPayRunRequest(string code, string name)
    {
        return new ScheduledPayRunRequest(code, name, "", 0, "", ScheduledPayRunRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPayRunRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPayRunRequestStatus next)
    {
        switch (Status)
        {
            case ScheduledPayRunRequestStatus.New:
                switch (next)
                {
                    case ScheduledPayRunRequestStatus.Done:
                    case ScheduledPayRunRequestStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPayRunRequestStatus.Queued:
                return next == ScheduledPayRunRequestStatus.Done || next == ScheduledPayRunRequestStatus.Failed;
            case ScheduledPayRunRequestStatus.Running:
                switch (next)
                {
                    case ScheduledPayRunRequestStatus.Done:
                        return true;
                }
                return false;
            case ScheduledPayRunRequestStatus.Done:
                return next == ScheduledPayRunRequestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledPayRunRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pay run request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)WindowDays;
    }

    public ScheduledPayRunRequest Copy()
    {
        return new ScheduledPayRunRequest(Code, Name, Region, WindowDays, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
