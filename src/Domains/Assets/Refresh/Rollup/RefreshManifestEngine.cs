namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh manifest calculation.</summary>
public interface IRefreshManifestModifier
{
    string Describe();
    double Adjust(RefreshManifest item, double valueValue);
}

public sealed record RefreshManifestLeaveDaysOffsetAdjuster(double Weight) : IRefreshManifestModifier
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(RefreshManifest item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record LabelRefreshManifestAdjuster(Dictionary<string, double> Factors) : IRefreshManifestModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RefreshManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredRefreshManifestAdjuster(List<double> Bounds, List<double> Rates) : IRefreshManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshManifest item, double valueValue)
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

public sealed record FlaggedRefreshManifestAdjuster(Registry Flags, string Flag, double Percent) : IRefreshManifestModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRefreshManifestAdjuster(double Step) : IRefreshManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the refresh manifest steps in order and keeps a trace of each.</summary>
public sealed class RefreshManifestEngine(List<IRefreshManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshManifestEngine DefaultRefreshManifestEngine(Registry flags)
    {
        return new RefreshManifestEngine(
            new List<IRefreshManifestModifier>
            {
                new RefreshManifestLeaveDaysOffsetAdjuster(1.99),
                new LabelRefreshManifestAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.35, ["south"] = 1.21, ["east"] = 1.15 }),
                new TieredRefreshManifestAdjuster(
                    new List<double> { 432.0, 3655.0, 3669.0, 6395.0 },
                    new List<double> { 1.0, 1.15, 1.19, 0.88 }),
                new FlaggedRefreshManifestAdjuster(flags, "refresh-manifest-regional", 6.3),
                new RoundingRefreshManifestAdjuster(5.0),
            },
            new List<string>(),
            2.5,
            68628.3);
    }

    public double Price(RefreshManifest item)
    {
        var current = item.EffectiveExposure();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<RefreshManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
