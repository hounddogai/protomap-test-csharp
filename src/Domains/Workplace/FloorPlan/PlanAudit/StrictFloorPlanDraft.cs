namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict floor plan draft is in its life cycle.</summary>
public enum StrictFloorPlanDraftStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A strict floor plan draft as the floor plan team keeps it.</summary>
public sealed class StrictFloorPlanDraft(string code, string label, double amount, int retries, int batchSize, double margin, int attempts, double discount, int createdAt, double rating, string team, StrictFloorPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public int BatchSize { get; set; } = batchSize;
    public double Margin { get; set; } = margin;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public int CreatedAt { get; set; } = createdAt;
    public double Rating { get; set; } = rating;
    public string Team { get; set; } = team;
    public StrictFloorPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a strict floor plan draft in status requested.</summary>
    public static StrictFloorPlanDraft NewStrictFloorPlanDraft(string code, string label)
    {
        return new StrictFloorPlanDraft(
            code,
            label,
            0.0,
            0,
            0,
            0.0,
            0,
            0.0,
            0,
            0.0,
            "",
            StrictFloorPlanDraftStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictFloorPlanDraftStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictFloorPlanDraftStatus next)
    {
        switch (Status)
        {
            case StrictFloorPlanDraftStatus.Requested:
                switch (next)
                {
                    case StrictFloorPlanDraftStatus.Granted:
                        return true;
                }
                return false;
            case StrictFloorPlanDraftStatus.Granted:
                return next == StrictFloorPlanDraftStatus.Revoked;
            case StrictFloorPlanDraftStatus.Revoked:
                switch (next)
                {
                    case StrictFloorPlanDraftStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictFloorPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict floor plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)Attempts - (double)Retries, 2);
    }

    public StrictFloorPlanDraft Copy()
    {
        return new StrictFloorPlanDraft(
            Code,
            Label,
            Amount,
            Retries,
            BatchSize,
            Margin,
            Attempts,
            Discount,
            CreatedAt,
            Rating,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
