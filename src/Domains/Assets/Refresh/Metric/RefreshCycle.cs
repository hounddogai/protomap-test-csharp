namespace Acme.Workforce.Assets.Refresh.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a refresh cycle is in its life cycle.</summary>
public enum RefreshCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One refresh cycle, with what the planners need to act on it.</summary>
public sealed class RefreshCycle(string code, string name, string channel, string description, int leaveDays, string jobCode, double floorValue, int quantity, RefreshCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public int LeaveDays { get; set; } = leaveDays;
    public string JobCode { get; set; } = jobCode;
    public double FloorValue { get; set; } = floorValue;
    public int Quantity { get; set; } = quantity;
    public RefreshCycleStatus Status { get; set; } = status;

    /// <summary>Starts a refresh cycle in status requested.</summary>
    public static RefreshCycle NewRefreshCycle(string code, string name)
    {
        return new RefreshCycle(code, name, "", "", 0, "", 0.0, 0, RefreshCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RefreshCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RefreshCycleStatus next)
    {
        switch (Status)
        {
            case RefreshCycleStatus.Requested:
                switch (next)
                {
                    case RefreshCycleStatus.Revoked:
                    case RefreshCycleStatus.Expired:
                        return true;
                }
                return false;
            case RefreshCycleStatus.Granted:
                switch (next)
                {
                    case RefreshCycleStatus.Revoked:
                    case RefreshCycleStatus.Expired:
                        return true;
                }
                return false;
            case RefreshCycleStatus.Revoked:
                return next == RefreshCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(RefreshCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"refresh cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"refresh cycle"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(FloorValue * (double)LeaveDays, 2);
    }

    public RefreshCycle Copy()
    {
        return new RefreshCycle(Code, Name, Channel, Description, LeaveDays, JobCode, FloorValue, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
