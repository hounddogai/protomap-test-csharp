namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted asset return window is in its life cycle.</summary>
public enum WeightedAssetReturnWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A weighted asset return window as the asset return team keeps it.</summary>
public sealed class WeightedAssetReturnWindow(string code, string name, string segment, string source, int batchSize, string currency, WeightedAssetReturnWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public int BatchSize { get; set; } = batchSize;
    public string Currency { get; set; } = currency;
    public WeightedAssetReturnWindowStatus Status { get; set; } = status;

    /// <summary>Starts a weighted asset return window in status proposed.</summary>
    public static WeightedAssetReturnWindow NewWeightedAssetReturnWindow(string code, string name)
    {
        return new WeightedAssetReturnWindow(code, name, "", "", 0, "", WeightedAssetReturnWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAssetReturnWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAssetReturnWindowStatus next)
    {
        switch (Status)
        {
            case WeightedAssetReturnWindowStatus.Proposed:
                return next == WeightedAssetReturnWindowStatus.Approved || next == WeightedAssetReturnWindowStatus.Withdrawn;
            case WeightedAssetReturnWindowStatus.Approved:
                switch (next)
                {
                    case WeightedAssetReturnWindowStatus.Rejected:
                    case WeightedAssetReturnWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedAssetReturnWindowStatus.Rejected:
                switch (next)
                {
                    case WeightedAssetReturnWindowStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedAssetReturnWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted asset return window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted asset return window"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public WeightedAssetReturnWindow Copy()
    {
        return new WeightedAssetReturnWindow(Code, Name, Segment, Source, BatchSize, Currency, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
