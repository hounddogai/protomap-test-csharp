namespace Acme.Workforce.Compliance.PolicyAck.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global policy ack bundle calculation.</summary>
public interface IGlobalPolicyAckBundleFactor
{
    string Describe();
    double Adjust(GlobalPolicyAckBundle item, double valueValue);
}

public sealed record TieredGlobalPolicyAckBundleAdjuster(List<double> Bounds, List<double> Rates) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
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

public sealed record TitleGlobalPolicyAckBundleAdjuster(Dictionary<string, double> Factors) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentGlobalPolicyAckBundleAdjuster(double Percent) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record GlobalPolicyAckBundleCreatedAtOffsetAdjuster(double Weight) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"plus created at times {Weight}";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
    {
        return valueValue + (double)item.CreatedAt * Weight;
    }
}

public sealed record RoundingGlobalPolicyAckBundleAdjuster(double Step) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGlobalPolicyAckBundleAdjuster(double Minimum) : IGlobalPolicyAckBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalPolicyAckBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global policy ack bundle steps in order and keeps a trace of each.</summary>
public sealed class GlobalPolicyAckBundleEngine(List<IGlobalPolicyAckBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalPolicyAckBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalPolicyAckBundleEngine DefaultGlobalPolicyAckBundleEngine()
    {
        return new GlobalPolicyAckBundleEngine(
            new List<IGlobalPolicyAckBundleFactor>
            {
                new TieredGlobalPolicyAckBundleAdjuster(
                    new List<double> { 5054.0, 8034.0 },
                    new List<double> { 1.01, 0.82 }),
                new TitleGlobalPolicyAckBundleAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.71, ["south"] = 1.21 }),
                new PercentGlobalPolicyAckBundleAdjuster(-9.2),
                new GlobalPolicyAckBundleCreatedAtOffsetAdjuster(0.26),
                new RoundingGlobalPolicyAckBundleAdjuster(0.1),
                new MinimumGlobalPolicyAckBundleAdjuster(28.6),
            },
            new List<string>(),
            2.2,
            60426.2);
    }

    public double Evaluate(GlobalPolicyAckBundle item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalPolicyAckBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
