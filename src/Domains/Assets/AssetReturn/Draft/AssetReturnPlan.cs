namespace Acme.Workforce.Assets.AssetReturn.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return plan is in its life cycle.</summary>
public enum AssetReturnPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A asset return plan. Its code is unique within a tenant.</summary>
public sealed class AssetReturnPlan(string code, string title, int revision, string notes, int limit, string reviewer, int batchSize, bool archived, AssetReturnPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public string Notes { get; set; } = notes;
    public int Limit { get; set; } = limit;
    public string Reviewer { get; set; } = reviewer;
    public int BatchSize { get; set; } = batchSize;
    public bool Archived { get; set; } = archived;
    public AssetReturnPlanStatus Status { get; set; } = status;

    /// <summary>Starts a asset return plan in status created.</summary>
    public static AssetReturnPlan NewAssetReturnPlan(string code, string title)
    {
        return new AssetReturnPlan(code, title, 0, "", 0, "", 0, false, AssetReturnPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnPlanStatus next)
    {
        switch (Status)
        {
            case AssetReturnPlanStatus.Created:
                return next == AssetReturnPlanStatus.Published;
            case AssetReturnPlanStatus.Validated:
                switch (next)
                {
                    case AssetReturnPlanStatus.Published:
                        return true;
                }
                return false;
            case AssetReturnPlanStatus.Published:
                switch (next)
                {
                    case AssetReturnPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"asset return plan"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Limit - (double)BatchSize, 2);
    }

    public AssetReturnPlan Copy()
    {
        return new AssetReturnPlan(Code, Title, Revision, Notes, Limit, Reviewer, BatchSize, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
