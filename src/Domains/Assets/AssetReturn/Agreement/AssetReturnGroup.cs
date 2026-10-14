namespace Acme.Workforce.Assets.AssetReturn.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return group is in its life cycle.</summary>
public enum AssetReturnGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One asset return group, with what the planners need to act on it.</summary>
public sealed class AssetReturnGroup(string code, string title, int version, int batchSize, int graceDays, AssetReturnGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public int GraceDays { get; set; } = graceDays;
    public AssetReturnGroupStatus Status { get; set; } = status;

    /// <summary>Starts a asset return group in status new.</summary>
    public static AssetReturnGroup NewAssetReturnGroup(string code, string title)
    {
        return new AssetReturnGroup(code, title, 0, 0, 0, AssetReturnGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnGroupStatus next)
    {
        switch (Status)
        {
            case AssetReturnGroupStatus.New:
                return next == AssetReturnGroupStatus.Done;
            case AssetReturnGroupStatus.Queued:
                return next == AssetReturnGroupStatus.Done || next == AssetReturnGroupStatus.Failed;
            case AssetReturnGroupStatus.Running:
                return next == AssetReturnGroupStatus.Failed;
            case AssetReturnGroupStatus.Done:
                switch (next)
                {
                    case AssetReturnGroupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"asset return group"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Version * (double)GraceDays, 2);
    }

    public AssetReturnGroup Copy()
    {
        return new AssetReturnGroup(Code, Title, Version, BatchSize, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
