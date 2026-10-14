namespace Acme.Workforce.Assets.Refresh.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled refresh cycle is in its life cycle.</summary>
public enum ScheduledRefreshCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A scheduled refresh cycle as the refresh team keeps it.</summary>
public sealed class ScheduledRefreshCycle(string code, string label, int durationMinutes, string channel, double amount, double fte, double ceiling, ScheduledRefreshCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Channel { get; set; } = channel;
    public double Amount { get; set; } = amount;
    public double Fte { get; set; } = fte;
    public double Ceiling { get; set; } = ceiling;
    public ScheduledRefreshCycleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled refresh cycle in status created.</summary>
    public static ScheduledRefreshCycle NewScheduledRefreshCycle(string code, string label)
    {
        return new ScheduledRefreshCycle(code, label, 0, "", 0.0, 0.0, 0.0, ScheduledRefreshCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRefreshCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRefreshCycleStatus next)
    {
        switch (Status)
        {
            case ScheduledRefreshCycleStatus.Created:
                return next == ScheduledRefreshCycleStatus.Published;
            case ScheduledRefreshCycleStatus.Validated:
                switch (next)
                {
                    case ScheduledRefreshCycleStatus.Published:
                        return true;
                }
                return false;
            case ScheduledRefreshCycleStatus.Published:
                return next == ScheduledRefreshCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledRefreshCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled refresh cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled refresh cycle"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Fte + Ceiling, 2);
    }

    public ScheduledRefreshCycle Copy()
    {
        return new ScheduledRefreshCycle(Code, Label, DurationMinutes, Channel, Amount, Fte, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
