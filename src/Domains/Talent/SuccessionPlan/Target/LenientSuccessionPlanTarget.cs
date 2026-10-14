namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan target is in its life cycle.</summary>
public enum LenientSuccessionPlanTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A lenient succession plan target. Its code is unique within a tenant.</summary>
public sealed class LenientSuccessionPlanTarget(string code, string title, double ratio, double budget, string summary, LenientSuccessionPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Budget { get; set; } = budget;
    public string Summary { get; set; } = summary;
    public LenientSuccessionPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan target in status planned.</summary>
    public static LenientSuccessionPlanTarget NewLenientSuccessionPlanTarget(string code, string title)
    {
        return new LenientSuccessionPlanTarget(code, title, 0.0, 0.0, "", LenientSuccessionPlanTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanTargetStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanTargetStatus.Planned:
                switch (next)
                {
                    case LenientSuccessionPlanTargetStatus.Scheduled:
                    case LenientSuccessionPlanTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientSuccessionPlanTargetStatus.Scheduled:
                switch (next)
                {
                    case LenientSuccessionPlanTargetStatus.InProgress:
                        return true;
                }
                return false;
            case LenientSuccessionPlanTargetStatus.InProgress:
                switch (next)
                {
                    case LenientSuccessionPlanTargetStatus.Completed:
                    case LenientSuccessionPlanTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientSuccessionPlanTargetStatus.Completed:
                return next == LenientSuccessionPlanTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient succession plan target"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Budget - Ratio, 2);
    }

    public LenientSuccessionPlanTarget Copy()
    {
        return new LenientSuccessionPlanTarget(Code, Title, Ratio, Budget, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
