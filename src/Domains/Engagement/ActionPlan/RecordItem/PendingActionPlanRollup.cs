namespace Acme.Workforce.Engagement.ActionPlan.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending action plan rollup is in its life cycle.</summary>
public enum PendingActionPlanRollupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending action plan rollup as the action plan team keeps it.</summary>
public sealed class PendingActionPlanRollup(string code, string label, double amount, string ownerId, string badge, int retries, PendingActionPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string OwnerId { get; set; } = ownerId;
    public string Badge { get; set; } = badge;
    public int Retries { get; set; } = retries;
    public PendingActionPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pending action plan rollup in status planned.</summary>
    public static PendingActionPlanRollup NewPendingActionPlanRollup(string code, string label)
    {
        return new PendingActionPlanRollup(code, label, 0.0, "", "", 0, PendingActionPlanRollupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingActionPlanRollupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingActionPlanRollupStatus next)
    {
        switch (Status)
        {
            case PendingActionPlanRollupStatus.Planned:
                return next == PendingActionPlanRollupStatus.Scheduled || next == PendingActionPlanRollupStatus.Completed;
            case PendingActionPlanRollupStatus.Scheduled:
                switch (next)
                {
                    case PendingActionPlanRollupStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingActionPlanRollupStatus.InProgress:
                return next == PendingActionPlanRollupStatus.Completed || next == PendingActionPlanRollupStatus.Cancelled;
            case PendingActionPlanRollupStatus.Completed:
                switch (next)
                {
                    case PendingActionPlanRollupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingActionPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending action plan rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Amount + (double)Retries, 2);
    }

    public PendingActionPlanRollup Copy()
    {
        return new PendingActionPlanRollup(Code, Label, Amount, OwnerId, Badge, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
