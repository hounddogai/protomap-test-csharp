namespace Acme.Workforce.Talent.Pool.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool baseline is in its life cycle.</summary>
public enum PoolBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pool baseline, with what the planners need to act on it.</summary>
public sealed class PoolBaseline(string code, string name, int priority, int endsAt, string employeeId, string title, string grade, string badge, PoolBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public string EmployeeId { get; set; } = employeeId;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string Badge { get; set; } = badge;
    public PoolBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a pool baseline in status planned.</summary>
    public static PoolBaseline NewPoolBaseline(string code, string name)
    {
        return new PoolBaseline(code, name, 0, 0, "", "", "", "", PoolBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolBaselineStatus next)
    {
        switch (Status)
        {
            case PoolBaselineStatus.Planned:
                return next == PoolBaselineStatus.Cancelled;
            case PoolBaselineStatus.Scheduled:
                return next == PoolBaselineStatus.InProgress;
            case PoolBaselineStatus.InProgress:
                switch (next)
                {
                    case PoolBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case PoolBaselineStatus.Completed:
                switch (next)
                {
                    case PoolBaselineStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)EndsAt - (double)Priority, 2);
    }

    public PoolBaseline Copy()
    {
        return new PoolBaseline(Code, Name, Priority, EndsAt, EmployeeId, Title, Grade, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
