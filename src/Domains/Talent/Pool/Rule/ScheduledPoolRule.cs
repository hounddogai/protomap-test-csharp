namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled pool rule is in its life cycle.</summary>
public enum ScheduledPoolRuleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled pool rule. Its code is unique within a tenant.</summary>
public sealed class ScheduledPoolRule(string code, string title, double minimum, bool visible, int durationMinutes, int capacity, int limit, int windowDays, double maximum, ScheduledPoolRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public bool Visible { get; set; } = visible;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Capacity { get; set; } = capacity;
    public int Limit { get; set; } = limit;
    public int WindowDays { get; set; } = windowDays;
    public double Maximum { get; set; } = maximum;
    public ScheduledPoolRuleStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled pool rule in status new.</summary>
    public static ScheduledPoolRule NewScheduledPoolRule(string code, string title)
    {
        return new ScheduledPoolRule(code, title, 0.0, false, 0, 0, 0, 0, 0.0, ScheduledPoolRuleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledPoolRuleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledPoolRuleStatus next)
    {
        switch (Status)
        {
            case ScheduledPoolRuleStatus.New:
                switch (next)
                {
                    case ScheduledPoolRuleStatus.Running:
                        return true;
                }
                return false;
            case ScheduledPoolRuleStatus.Queued:
                switch (next)
                {
                    case ScheduledPoolRuleStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledPoolRuleStatus.Running:
                switch (next)
                {
                    case ScheduledPoolRuleStatus.Done:
                        return true;
                }
                return false;
            case ScheduledPoolRuleStatus.Done:
                return next == ScheduledPoolRuleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledPoolRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled pool rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round(Maximum * (double)DurationMinutes, 2);
    }

    public ScheduledPoolRule Copy()
    {
        return new ScheduledPoolRule(
            Code,
            Title,
            Minimum,
            Visible,
            DurationMinutes,
            Capacity,
            Limit,
            WindowDays,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
