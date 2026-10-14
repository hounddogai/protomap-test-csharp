namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling accrual window is in its life cycle.</summary>
public enum RollingAccrualWindowStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One rolling accrual window, with what the planners need to act on it.</summary>
public sealed class RollingAccrualWindow(string code, string title, double cost, int durationMinutes, string unit, string managerId, RollingAccrualWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Unit { get; set; } = unit;
    public string ManagerId { get; set; } = managerId;
    public RollingAccrualWindowStatus Status { get; set; } = status;

    /// <summary>Starts a rolling accrual window in status requested.</summary>
    public static RollingAccrualWindow NewRollingAccrualWindow(string code, string title)
    {
        return new RollingAccrualWindow(code, title, 0.0, 0, "", "", RollingAccrualWindowStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccrualWindowStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccrualWindowStatus next)
    {
        switch (Status)
        {
            case RollingAccrualWindowStatus.Requested:
                return next == RollingAccrualWindowStatus.Revoked;
            case RollingAccrualWindowStatus.Granted:
                switch (next)
                {
                    case RollingAccrualWindowStatus.Revoked:
                    case RollingAccrualWindowStatus.Expired:
                        return true;
                }
                return false;
            case RollingAccrualWindowStatus.Revoked:
                switch (next)
                {
                    case RollingAccrualWindowStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingAccrualWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling accrual window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedLoad()
    {
        return Math.Round(Cost + (double)DurationMinutes, 2);
    }

    public RollingAccrualWindow Copy()
    {
        return new RollingAccrualWindow(Code, Title, Cost, DurationMinutes, Unit, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
