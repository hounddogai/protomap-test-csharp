namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return bundle is in its life cycle.</summary>
public enum AssetReturnBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a asset return bundle and its current status.</summary>
public sealed class AssetReturnBundle(string code, string name, string costCenter, int batchSize, string department, double budget, string skill, int graceDays, bool active, double cost, AssetReturnBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public int BatchSize { get; set; } = batchSize;
    public string Department { get; set; } = department;
    public double Budget { get; set; } = budget;
    public string Skill { get; set; } = skill;
    public int GraceDays { get; set; } = graceDays;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public AssetReturnBundleStatus Status { get; set; } = status;

    /// <summary>Starts a asset return bundle in status new.</summary>
    public static AssetReturnBundle NewAssetReturnBundle(string code, string name)
    {
        return new AssetReturnBundle(code, name, "", 0, "", 0.0, "", 0, false, 0.0, AssetReturnBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnBundleStatus next)
    {
        switch (Status)
        {
            case AssetReturnBundleStatus.New:
                switch (next)
                {
                    case AssetReturnBundleStatus.Queued:
                    case AssetReturnBundleStatus.Running:
                        return true;
                }
                return false;
            case AssetReturnBundleStatus.Queued:
                return next == AssetReturnBundleStatus.Running || next == AssetReturnBundleStatus.Failed;
            case AssetReturnBundleStatus.Running:
                switch (next)
                {
                    case AssetReturnBundleStatus.Done:
                        return true;
                }
                return false;
            case AssetReturnBundleStatus.Done:
                switch (next)
                {
                    case AssetReturnBundleStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Cost + Budget, 2);
    }

    public AssetReturnBundle Copy()
    {
        return new AssetReturnBundle(
            Code,
            Name,
            CostCenter,
            BatchSize,
            Department,
            Budget,
            Skill,
            GraceDays,
            Active,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
