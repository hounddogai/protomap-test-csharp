namespace Acme.Workforce.Assets.AssetReturn.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched asset return budget is in its life cycle.</summary>
public enum BatchedAssetReturnBudgetStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched asset return budget as the asset return team keeps it.</summary>
public sealed class BatchedAssetReturnBudget(string code, string title, string description, int endsAt, string phone, string segment, string notes, double ratio, bool active, BatchedAssetReturnBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public int EndsAt { get; set; } = endsAt;
    public string Phone { get; set; } = phone;
    public string Segment { get; set; } = segment;
    public string Notes { get; set; } = notes;
    public double Ratio { get; set; } = ratio;
    public bool Active { get; set; } = active;
    public BatchedAssetReturnBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a batched asset return budget in status open.</summary>
    public static BatchedAssetReturnBudget NewBatchedAssetReturnBudget(string code, string title)
    {
        return new BatchedAssetReturnBudget(
            code,
            title,
            "",
            0,
            "",
            "",
            "",
            0.0,
            false,
            BatchedAssetReturnBudgetStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAssetReturnBudgetStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAssetReturnBudgetStatus next)
    {
        switch (Status)
        {
            case BatchedAssetReturnBudgetStatus.Open:
                return next == BatchedAssetReturnBudgetStatus.Reopened || next == BatchedAssetReturnBudgetStatus.Archived;
            case BatchedAssetReturnBudgetStatus.InReview:
                switch (next)
                {
                    case BatchedAssetReturnBudgetStatus.Archived:
                        return true;
                }
                return false;
            case BatchedAssetReturnBudgetStatus.Resolved:
                switch (next)
                {
                    case BatchedAssetReturnBudgetStatus.Reopened:
                    case BatchedAssetReturnBudgetStatus.Archived:
                        return true;
                }
                return false;
            case BatchedAssetReturnBudgetStatus.Reopened:
                switch (next)
                {
                    case BatchedAssetReturnBudgetStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAssetReturnBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched asset return budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched asset return budget"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)EndsAt - Ratio, 2);
    }

    public BatchedAssetReturnBudget Copy()
    {
        return new BatchedAssetReturnBudget(
            Code,
            Title,
            Description,
            EndsAt,
            Phone,
            Segment,
            Notes,
            Ratio,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
