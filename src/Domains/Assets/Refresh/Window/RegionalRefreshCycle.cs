namespace Acme.Workforce.Assets.Refresh.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional refresh cycle is in its life cycle.</summary>
public enum RegionalRefreshCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a regional refresh cycle and its current status.</summary>
public sealed class RegionalRefreshCycle(string code, string name, int capacity, string jobCode, double fte, double minimum, string phone, int version, double ceiling, int leaveDays, RegionalRefreshCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string JobCode { get; set; } = jobCode;
    public double Fte { get; set; } = fte;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public int Version { get; set; } = version;
    public double Ceiling { get; set; } = ceiling;
    public int LeaveDays { get; set; } = leaveDays;
    public RegionalRefreshCycleStatus Status { get; set; } = status;

    /// <summary>Starts a regional refresh cycle in status idle.</summary>
    public static RegionalRefreshCycle NewRegionalRefreshCycle(string code, string name)
    {
        return new RegionalRefreshCycle(code, name, 0, "", 0.0, 0.0, "", 0, 0.0, 0, RegionalRefreshCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalRefreshCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalRefreshCycleStatus next)
    {
        switch (Status)
        {
            case RegionalRefreshCycleStatus.Idle:
                switch (next)
                {
                    case RegionalRefreshCycleStatus.Ready:
                        return true;
                }
                return false;
            case RegionalRefreshCycleStatus.Warming:
                switch (next)
                {
                    case RegionalRefreshCycleStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalRefreshCycleStatus.Ready:
                return next == RegionalRefreshCycleStatus.Draining || next == RegionalRefreshCycleStatus.Stopped;
            case RegionalRefreshCycleStatus.Draining:
                switch (next)
                {
                    case RegionalRefreshCycleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalRefreshCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional refresh cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"regional refresh cycle"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Fte - Ceiling, 2);
    }

    public RegionalRefreshCycle Copy()
    {
        return new RegionalRefreshCycle(
            Code,
            Name,
            Capacity,
            JobCode,
            Fte,
            Minimum,
            Phone,
            Version,
            Ceiling,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
