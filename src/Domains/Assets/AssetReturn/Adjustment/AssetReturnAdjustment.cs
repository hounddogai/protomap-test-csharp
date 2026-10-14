namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return adjustment is in its life cycle.</summary>
public enum AssetReturnAdjustmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A asset return adjustment as the asset return team keeps it.</summary>
public sealed class AssetReturnAdjustment(string code, string title, string checksum, string unit, int updatedAt, int startsAt, AssetReturnAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public int StartsAt { get; set; } = startsAt;
    public AssetReturnAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a asset return adjustment in status new.</summary>
    public static AssetReturnAdjustment NewAssetReturnAdjustment(string code, string title)
    {
        return new AssetReturnAdjustment(code, title, "", "", 0, 0, AssetReturnAdjustmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnAdjustmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnAdjustmentStatus next)
    {
        switch (Status)
        {
            case AssetReturnAdjustmentStatus.New:
                return next == AssetReturnAdjustmentStatus.Done || next == AssetReturnAdjustmentStatus.Failed;
            case AssetReturnAdjustmentStatus.Queued:
                return next == AssetReturnAdjustmentStatus.Running || next == AssetReturnAdjustmentStatus.Done;
            case AssetReturnAdjustmentStatus.Running:
                return next == AssetReturnAdjustmentStatus.Failed;
            case AssetReturnAdjustmentStatus.Done:
                return next == AssetReturnAdjustmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(AssetReturnAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"asset return adjustment"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)StartsAt + (double)UpdatedAt, 2);
    }

    public AssetReturnAdjustment Copy()
    {
        return new AssetReturnAdjustment(Code, Title, Checksum, Unit, UpdatedAt, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
