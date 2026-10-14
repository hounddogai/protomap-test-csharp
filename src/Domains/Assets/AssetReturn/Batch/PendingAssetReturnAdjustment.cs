namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending asset return adjustment is in its life cycle.</summary>
public enum PendingAssetReturnAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending asset return adjustment as the asset return team keeps it.</summary>
public sealed class PendingAssetReturnAdjustment(string code, string title, int quantity, string channel, string reviewer, double budget, string description, PendingAssetReturnAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Channel { get; set; } = channel;
    public string Reviewer { get; set; } = reviewer;
    public double Budget { get; set; } = budget;
    public string Description { get; set; } = description;
    public PendingAssetReturnAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending asset return adjustment in status new.</summary>
    public static PendingAssetReturnAdjustment NewPendingAssetReturnAdjustment(string code, string title)
    {
        return new PendingAssetReturnAdjustment(
            code,
            title,
            0,
            "",
            "",
            0.0,
            "",
            PendingAssetReturnAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingAssetReturnAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingAssetReturnAdjustmentStatus next)
    {
        switch (Status)
        {
            case PendingAssetReturnAdjustmentStatus.New:
                return next == PendingAssetReturnAdjustmentStatus.Running;
            case PendingAssetReturnAdjustmentStatus.Queued:
                switch (next)
                {
                    case PendingAssetReturnAdjustmentStatus.Running:
                        return true;
                }
                return false;
            case PendingAssetReturnAdjustmentStatus.Running:
                return next == PendingAssetReturnAdjustmentStatus.Failed;
            case PendingAssetReturnAdjustmentStatus.Done:
                return next == PendingAssetReturnAdjustmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingAssetReturnAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending asset return adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending asset return adjustment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Budget * (double)Quantity, 2);
    }

    public PendingAssetReturnAdjustment Copy()
    {
        return new PendingAssetReturnAdjustment(
            Code,
            Title,
            Quantity,
            Channel,
            Reviewer,
            Budget,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
