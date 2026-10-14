namespace Acme.Workforce.Talent.Pool.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool window is in its life cycle.</summary>
public enum PoolWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pool window, with what the planners need to act on it.</summary>
public sealed class PoolWindow(string code, string title, double hours, string managerId, int startsAt, double weight, string badge, bool visible, PoolWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string ManagerId { get; set; } = managerId;
    public int StartsAt { get; set; } = startsAt;
    public double Weight { get; set; } = weight;
    public string Badge { get; set; } = badge;
    public bool Visible { get; set; } = visible;
    public PoolWindowStatus Status { get; set; } = status;

    /// <summary>Starts a pool window in status planned.</summary>
    public static PoolWindow NewPoolWindow(string code, string title)
    {
        return new PoolWindow(code, title, 0.0, "", 0, 0.0, "", false, PoolWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolWindowStatus next)
    {
        switch (Status)
        {
            case PoolWindowStatus.Planned:
                return next == PoolWindowStatus.Completed;
            case PoolWindowStatus.Scheduled:
                switch (next)
                {
                    case PoolWindowStatus.InProgress:
                    case PoolWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolWindowStatus.InProgress:
                return next == PoolWindowStatus.Completed || next == PoolWindowStatus.Cancelled;
            case PoolWindowStatus.Completed:
                switch (next)
                {
                    case PoolWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pool window"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt - Hours, 2);
    }

    public PoolWindow Copy()
    {
        return new PoolWindow(Code, Title, Hours, ManagerId, StartsAt, Weight, Badge, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
