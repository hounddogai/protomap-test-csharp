namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled accrual baseline is in its life cycle.</summary>
public enum ScheduledAccrualBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled accrual baseline as the accrual team keeps it.</summary>
public sealed class ScheduledAccrualBaseline(string code, string title, int quantity, string channel, double threshold, ScheduledAccrualBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public double Threshold { get; set; } = threshold;
    public ScheduledAccrualBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled accrual baseline in status idle.</summary>
    public static ScheduledAccrualBaseline NewScheduledAccrualBaseline(string code, string title)
    {
        return new ScheduledAccrualBaseline(code, title, 0, "", 0.0, ScheduledAccrualBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledAccrualBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledAccrualBaselineStatus next)
    {
        switch (Status)
        {
            case ScheduledAccrualBaselineStatus.Idle:
                switch (next)
                {
                    case ScheduledAccrualBaselineStatus.Draining:
                    case ScheduledAccrualBaselineStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledAccrualBaselineStatus.Warming:
                return next == ScheduledAccrualBaselineStatus.Ready;
            case ScheduledAccrualBaselineStatus.Ready:
                return next == ScheduledAccrualBaselineStatus.Draining;
            case ScheduledAccrualBaselineStatus.Draining:
                return next == ScheduledAccrualBaselineStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ScheduledAccrualBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled accrual baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Quantity * Threshold, 2);
    }

    public ScheduledAccrualBaseline Copy()
    {
        return new ScheduledAccrualBaseline(Code, Title, Quantity, Channel, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
