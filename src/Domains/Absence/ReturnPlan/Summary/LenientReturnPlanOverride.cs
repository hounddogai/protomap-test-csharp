namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient return plan override is in its life cycle.</summary>
public enum LenientReturnPlanOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One lenient return plan override, with what the planners need to act on it.</summary>
public sealed class LenientReturnPlanOverride(string code, string name, int retries, string source, double score, string title, LenientReturnPlanOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string Source { get; set; } = source;
    public double Score { get; set; } = score;
    public string Title { get; set; } = title;
    public LenientReturnPlanOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a lenient return plan override in status planned.</summary>
    public static LenientReturnPlanOverride NewLenientReturnPlanOverride(string code, string name)
    {
        return new LenientReturnPlanOverride(code, name, 0, "", 0.0, "", LenientReturnPlanOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientReturnPlanOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientReturnPlanOverrideStatus next)
    {
        switch (Status)
        {
            case LenientReturnPlanOverrideStatus.Planned:
                return next == LenientReturnPlanOverrideStatus.Scheduled || next == LenientReturnPlanOverrideStatus.InProgress;
            case LenientReturnPlanOverrideStatus.Scheduled:
                return next == LenientReturnPlanOverrideStatus.InProgress;
            case LenientReturnPlanOverrideStatus.InProgress:
                return next == LenientReturnPlanOverrideStatus.Completed || next == LenientReturnPlanOverrideStatus.Cancelled;
            case LenientReturnPlanOverrideStatus.Completed:
                return next == LenientReturnPlanOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientReturnPlanOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient return plan override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Score + (double)Retries, 2);
    }

    public LenientReturnPlanOverride Copy()
    {
        return new LenientReturnPlanOverride(Code, Name, Retries, Source, Score, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
