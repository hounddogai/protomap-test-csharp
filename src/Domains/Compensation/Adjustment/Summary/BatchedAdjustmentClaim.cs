namespace Acme.Workforce.Compensation.Adjustment.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched adjustment claim is in its life cycle.</summary>
public enum BatchedAdjustmentClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a batched adjustment claim and its current status.</summary>
public sealed class BatchedAdjustmentClaim(string code, string title, double rate, string notes, string location, double floorValue, int retries, BatchedAdjustmentClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Notes { get; set; } = notes;
    public string Location { get; set; } = location;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public BatchedAdjustmentClaimStatus Status { get; set; } = status;

    /// <summary>Starts a batched adjustment claim in status created.</summary>
    public static BatchedAdjustmentClaim NewBatchedAdjustmentClaim(string code, string title)
    {
        return new BatchedAdjustmentClaim(code, title, 0.0, "", "", 0.0, 0, BatchedAdjustmentClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAdjustmentClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAdjustmentClaimStatus next)
    {
        switch (Status)
        {
            case BatchedAdjustmentClaimStatus.Created:
                return next == BatchedAdjustmentClaimStatus.Validated;
            case BatchedAdjustmentClaimStatus.Validated:
                return next == BatchedAdjustmentClaimStatus.Retired;
            case BatchedAdjustmentClaimStatus.Published:
                return next == BatchedAdjustmentClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(BatchedAdjustmentClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched adjustment claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched adjustment claim"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Retries - FloorValue, 2);
    }

    public BatchedAdjustmentClaim Copy()
    {
        return new BatchedAdjustmentClaim(Code, Title, Rate, Notes, Location, FloorValue, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
