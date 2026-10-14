namespace Acme.Workforce.Orgdesign.CostModel.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default cost model manifest calculation.</summary>
public interface IDefaultCostModelManifestFactor
{
    string Describe();
    double Adjust(DefaultCostModelManifest item, double valueValue);
}

public sealed record CountryDefaultCostModelManifestAdjuster(Dictionary<string, double> Factors) : IDefaultCostModelManifestFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultCostModelManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingDefaultCostModelManifestAdjuster(double Step) : IDefaultCostModelManifestFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultCostModelManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultCostModelManifestRetriesOffsetAdjuster(double Weight) : IDefaultCostModelManifestFactor
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(DefaultCostModelManifest item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumDefaultCostModelManifestAdjuster(double Minimum) : IDefaultCostModelManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultCostModelManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the default cost model manifest steps in order and keeps a trace of each.</summary>
public sealed class DefaultCostModelManifestEngine(List<IDefaultCostModelManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultCostModelManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultCostModelManifestEngine DefaultDefaultCostModelManifestEngine()
    {
        return new DefaultCostModelManifestEngine(
            new List<IDefaultCostModelManifestFactor>
            {
                new CountryDefaultCostModelManifestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.72, ["retail"] = 1.05 }),
                new RoundingDefaultCostModelManifestAdjuster(1.0),
                new DefaultCostModelManifestRetriesOffsetAdjuster(0.1),
                new MinimumDefaultCostModelManifestAdjuster(37.2),
            },
            new List<string>(),
            2.7,
            29184.5);
    }

    public double Compute(DefaultCostModelManifest item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultCostModelManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
