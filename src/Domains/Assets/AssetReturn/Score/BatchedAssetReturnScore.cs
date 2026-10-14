namespace Acme.Workforce.Assets.AssetReturn.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched asset return score is in its life cycle.</summary>
public enum BatchedAssetReturnScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A batched asset return score as the asset return team keeps it.</summary>
public sealed class BatchedAssetReturnScore(string code, string name, double budget, string region, bool archived, BatchedAssetReturnScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public BatchedAssetReturnScoreStatus Status { get; set; } = status;

    /// <summary>Starts a batched asset return score in status created.</summary>
    public static BatchedAssetReturnScore NewBatchedAssetReturnScore(string code, string name)
    {
        return new BatchedAssetReturnScore(code, name, 0.0, "", false, BatchedAssetReturnScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAssetReturnScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAssetReturnScoreStatus next)
    {
        switch (Status)
        {
            case BatchedAssetReturnScoreStatus.Created:
                return next == BatchedAssetReturnScoreStatus.Published || next == BatchedAssetReturnScoreStatus.Retired;
            case BatchedAssetReturnScoreStatus.Validated:
                return next == BatchedAssetReturnScoreStatus.Published || next == BatchedAssetReturnScoreStatus.Retired;
            case BatchedAssetReturnScoreStatus.Published:
                switch (next)
                {
                    case BatchedAssetReturnScoreStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedAssetReturnScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched asset return score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public BatchedAssetReturnScore Copy()
    {
        return new BatchedAssetReturnScore(Code, Name, Budget, Region, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
