namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg tier calculation.</summary>
public interface IReorgTierFactor
{
    string Describe();
    double Adjust(ReorgTier item, double valueValue);
}

public sealed record PercentReorgTierAdjuster(double Percent) : IReorgTierFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReorgTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ReorgTierWindowDaysOffsetAdjuster(double Weight) : IReorgTierFactor
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(ReorgTier item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record MinimumReorgTierAdjuster(double Minimum) : IReorgTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the reorg tier steps in order and keeps a trace of each.</summary>
public sealed class ReorgTierEngine(List<IReorgTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgTierEngine DefaultReorgTierEngine()
    {
        return new ReorgTierEngine(
            new List<IReorgTierFactor>
            {
                new PercentReorgTierAdjuster(-14.7),
                new ReorgTierWindowDaysOffsetAdjuster(1.24),
                new MinimumReorgTierAdjuster(15.3),
            },
            new List<string>(),
            3.0,
            52089.5);
    }

    public double Price(ReorgTier item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
