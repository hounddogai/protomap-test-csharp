namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending succession plan target is in its life cycle.</summary>
public enum PendingSuccessionPlanTargetStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A pending succession plan target. Its code is unique within a tenant.</summary>
public sealed class PendingSuccessionPlanTarget(string code, string label, string reviewer, string notes, int batchSize, double budget, PendingSuccessionPlanTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public int BatchSize { get; set; } = batchSize;
    public double Budget { get; set; } = budget;
    public PendingSuccessionPlanTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pending succession plan target in status requested.</summary>
    public static PendingSuccessionPlanTarget NewPendingSuccessionPlanTarget(string code, string label)
    {
        return new PendingSuccessionPlanTarget(
            code,
            label,
            "",
            "",
            0,
            0.0,
            PendingSuccessionPlanTargetStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSuccessionPlanTargetStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSuccessionPlanTargetStatus next)
    {
        switch (Status)
        {
            case PendingSuccessionPlanTargetStatus.Requested:
                switch (next)
                {
                    case PendingSuccessionPlanTargetStatus.Expired:
                        return true;
                }
                return false;
            case PendingSuccessionPlanTargetStatus.Granted:
                return next == PendingSuccessionPlanTargetStatus.Revoked || next == PendingSuccessionPlanTargetStatus.Expired;
            case PendingSuccessionPlanTargetStatus.Revoked:
                return next == PendingSuccessionPlanTargetStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PendingSuccessionPlanTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending succession plan target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Budget * (double)BatchSize, 2);
    }

    public PendingSuccessionPlanTarget Copy()
    {
        return new PendingSuccessionPlanTarget(Code, Label, Reviewer, Notes, BatchSize, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
