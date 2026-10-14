namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict return plan rollup is in its life cycle.</summary>
public enum StrictReturnPlanRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict return plan rollup as the return plan team keeps it.</summary>
public sealed class StrictReturnPlanRollup(string code, string title, string currency, string timezone, string source, double threshold, double margin, bool active, StrictReturnPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public double Margin { get; set; } = margin;
    public bool Active { get; set; } = active;
    public StrictReturnPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a strict return plan rollup in status planned.</summary>
    public static StrictReturnPlanRollup NewStrictReturnPlanRollup(string code, string title)
    {
        return new StrictReturnPlanRollup(
            code,
            title,
            "",
            "",
            "",
            0.0,
            0.0,
            false,
            StrictReturnPlanRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReturnPlanRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReturnPlanRollupStatus next)
    {
        switch (Status)
        {
            case StrictReturnPlanRollupStatus.Planned:
                switch (next)
                {
                    case StrictReturnPlanRollupStatus.InProgress:
                        return true;
                }
                return false;
            case StrictReturnPlanRollupStatus.Scheduled:
                return next == StrictReturnPlanRollupStatus.Cancelled;
            case StrictReturnPlanRollupStatus.InProgress:
                switch (next)
                {
                    case StrictReturnPlanRollupStatus.Completed:
                    case StrictReturnPlanRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictReturnPlanRollupStatus.Completed:
                switch (next)
                {
                    case StrictReturnPlanRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictReturnPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict return plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Margin - Threshold, 2);
    }

    public StrictReturnPlanRollup Copy()
    {
        return new StrictReturnPlanRollup(
            Code,
            Title,
            Currency,
            Timezone,
            Source,
            Threshold,
            Margin,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
