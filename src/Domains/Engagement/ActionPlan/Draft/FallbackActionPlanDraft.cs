namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback action plan draft is in its life cycle.</summary>
public enum FallbackActionPlanDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A fallback action plan draft. Its code is unique within a tenant.</summary>
public sealed class FallbackActionPlanDraft(string code, string label, string grade, double amount, int quantity, int batchSize, FallbackActionPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public int Quantity { get; set; } = quantity;
    public int BatchSize { get; set; } = batchSize;
    public FallbackActionPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a fallback action plan draft in status requested.</summary>
    public static FallbackActionPlanDraft NewFallbackActionPlanDraft(string code, string label)
    {
        return new FallbackActionPlanDraft(code, label, "", 0.0, 0, 0, FallbackActionPlanDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackActionPlanDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackActionPlanDraftStatus next)
    {
        switch (Status)
        {
            case FallbackActionPlanDraftStatus.Requested:
                return next == FallbackActionPlanDraftStatus.Expired;
            case FallbackActionPlanDraftStatus.Granted:
                switch (next)
                {
                    case FallbackActionPlanDraftStatus.Revoked:
                        return true;
                }
                return false;
            case FallbackActionPlanDraftStatus.Revoked:
                switch (next)
                {
                    case FallbackActionPlanDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackActionPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback action plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)BatchSize - (double)Quantity, 2);
    }

    public FallbackActionPlanDraft Copy()
    {
        return new FallbackActionPlanDraft(Code, Label, Grade, Amount, Quantity, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
