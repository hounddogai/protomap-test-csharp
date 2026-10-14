namespace Acme.Workforce.Absence.LeaveRequest.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered leave request plan is in its life cycle.</summary>
public enum TieredLeaveRequestPlanStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a tiered leave request plan and its current status.</summary>
public sealed class TieredLeaveRequestPlan(string code, string label, double weight, string skill, string reviewer, int attempts, double amount, TieredLeaveRequestPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public int Attempts { get; set; } = attempts;
    public double Amount { get; set; } = amount;
    public TieredLeaveRequestPlanStatus Status { get; set; } = status;

    /// <summary>Starts a tiered leave request plan in status new.</summary>
    public static TieredLeaveRequestPlan NewTieredLeaveRequestPlan(string code, string label)
    {
        return new TieredLeaveRequestPlan(code, label, 0.0, "", "", 0, 0.0, TieredLeaveRequestPlanStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredLeaveRequestPlanStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredLeaveRequestPlanStatus next)
    {
        switch (Status)
        {
            case TieredLeaveRequestPlanStatus.New:
                return next == TieredLeaveRequestPlanStatus.Done;
            case TieredLeaveRequestPlanStatus.Queued:
                return next == TieredLeaveRequestPlanStatus.Failed;
            case TieredLeaveRequestPlanStatus.Running:
                return next == TieredLeaveRequestPlanStatus.Done;
            case TieredLeaveRequestPlanStatus.Done:
                return next == TieredLeaveRequestPlanStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TieredLeaveRequestPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered leave request plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Amount + (double)Attempts, 2);
    }

    public TieredLeaveRequestPlan Copy()
    {
        return new TieredLeaveRequestPlan(Code, Label, Weight, Skill, Reviewer, Attempts, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
