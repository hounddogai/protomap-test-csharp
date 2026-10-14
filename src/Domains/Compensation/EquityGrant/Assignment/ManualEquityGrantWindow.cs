namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual equity grant window is in its life cycle.</summary>
public enum ManualEquityGrantWindowStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a manual equity grant window and its current status.</summary>
public sealed class ManualEquityGrantWindow(string code, string title, string phone, int durationMinutes, string badge, string team, double score, bool active, int leaveDays, int priority, ManualEquityGrantWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Phone { get; set; } = phone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public bool Active { get; set; } = active;
    public int LeaveDays { get; set; } = leaveDays;
    public int Priority { get; set; } = priority;
    public ManualEquityGrantWindowStatus Status { get; set; } = status;

    /// <summary>Starts a manual equity grant window in status draft.</summary>
    public static ManualEquityGrantWindow NewManualEquityGrantWindow(string code, string title)
    {
        return new ManualEquityGrantWindow(
            code,
            title,
            "",
            0,
            "",
            "",
            0.0,
            false,
            0,
            0,
            ManualEquityGrantWindowStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualEquityGrantWindowStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualEquityGrantWindowStatus next)
    {
        switch (Status)
        {
            case ManualEquityGrantWindowStatus.Draft:
                switch (next)
                {
                    case ManualEquityGrantWindowStatus.Active:
                    case ManualEquityGrantWindowStatus.Closed:
                        return true;
                }
                return false;
            case ManualEquityGrantWindowStatus.Pending:
                return next == ManualEquityGrantWindowStatus.Active;
            case ManualEquityGrantWindowStatus.Active:
                switch (next)
                {
                    case ManualEquityGrantWindowStatus.Suspended:
                    case ManualEquityGrantWindowStatus.Closed:
                        return true;
                }
                return false;
            case ManualEquityGrantWindowStatus.Suspended:
                return next == ManualEquityGrantWindowStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualEquityGrantWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual equity grant window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Score * (double)Priority, 2);
    }

    public ManualEquityGrantWindow Copy()
    {
        return new ManualEquityGrantWindow(
            Code,
            Title,
            Phone,
            DurationMinutes,
            Badge,
            Team,
            Score,
            Active,
            LeaveDays,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
