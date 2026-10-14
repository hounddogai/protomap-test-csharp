namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient action plan baseline is in its life cycle.</summary>
public enum LenientActionPlanBaselineStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A lenient action plan baseline as the action plan team keeps it.</summary>
public sealed class LenientActionPlanBaseline(string code, string label, string notes, int updatedAt, double cost, LenientActionPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Cost { get; set; } = cost;
    public LenientActionPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a lenient action plan baseline in status idle.</summary>
    public static LenientActionPlanBaseline NewLenientActionPlanBaseline(string code, string label)
    {
        return new LenientActionPlanBaseline(code, label, "", 0, 0.0, LenientActionPlanBaselineStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientActionPlanBaselineStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientActionPlanBaselineStatus next)
    {
        switch (Status)
        {
            case LenientActionPlanBaselineStatus.Idle:
                return next == LenientActionPlanBaselineStatus.Warming || next == LenientActionPlanBaselineStatus.Stopped;
            case LenientActionPlanBaselineStatus.Warming:
                return next == LenientActionPlanBaselineStatus.Ready || next == LenientActionPlanBaselineStatus.Stopped;
            case LenientActionPlanBaselineStatus.Ready:
                switch (next)
                {
                    case LenientActionPlanBaselineStatus.Draining:
                        return true;
                }
                return false;
            case LenientActionPlanBaselineStatus.Draining:
                switch (next)
                {
                    case LenientActionPlanBaselineStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientActionPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient action plan baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"lenient action plan baseline"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)UpdatedAt + Cost, 2);
    }

    public LenientActionPlanBaseline Copy()
    {
        return new LenientActionPlanBaseline(Code, Label, Notes, UpdatedAt, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
