namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending asset return record calculation.</summary>
public interface IPendingAssetReturnRecordAdjuster
{
    string Describe();
    double Adjust(PendingAssetReturnRecord item, double valueValue);
}

public sealed record MinimumPendingAssetReturnRecordAdjuster(double Minimum) : IPendingAssetReturnRecordAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingAssetReturnRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingAssetReturnRecordAdjuster(List<double> Bounds, List<double> Rates) : IPendingAssetReturnRecordAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingAssetReturnRecord item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record TitlePendingAssetReturnRecordAdjuster(Dictionary<string, double> Factors) : IPendingAssetReturnRecordAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingAssetReturnRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPendingAssetReturnRecordAdjuster(Registry Flags, string Flag, double Percent) : IPendingAssetReturnRecordAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAssetReturnRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingAssetReturnRecordAdjuster(double Step) : IPendingAssetReturnRecordAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingAssetReturnRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending asset return record steps in order and keeps a trace of each.</summary>
public sealed class PendingAssetReturnRecordEngine(List<IPendingAssetReturnRecordAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAssetReturnRecordAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAssetReturnRecordEngine DefaultPendingAssetReturnRecordEngine(Registry flags)
    {
        return new PendingAssetReturnRecordEngine(
            new List<IPendingAssetReturnRecordAdjuster>
            {
                new MinimumPendingAssetReturnRecordAdjuster(36.8),
                new TieredPendingAssetReturnRecordAdjuster(
                    new List<double> { 472.0, 3574.0, 5929.0, 7972.0 },
                    new List<double> { 0.88, 0.81, 0.94, 0.92 }),
                new TitlePendingAssetReturnRecordAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.87, ["south"] = 1.01 }),
                new FlaggedPendingAssetReturnRecordAdjuster(flags, "pending-asset-return-record-manual", 3.6),
                new RoundingPendingAssetReturnRecordAdjuster(0.25),
            },
            new List<string>(),
            0.1,
            52847.5);
    }

    public double Price(PendingAssetReturnRecord item)
    {
        var current = item.MeasuredValue();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<PendingAssetReturnRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
