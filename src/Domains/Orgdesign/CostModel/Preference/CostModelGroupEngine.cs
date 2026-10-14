namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model group calculation.</summary>
public interface ICostModelGroupAdjuster
{
    string Describe();
    double Adjust(CostModelGroup item, double valueValue);
}

public sealed record RoundingCostModelGroupAdjuster(double Step) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCostModelGroupAdjuster(double Percent) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CostModelGroupCeilingOffsetAdjuster(double Weight) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(CostModelGroup item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumCostModelGroupAdjuster(double Minimum) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCostModelGroupAdjuster(List<double> Bounds, List<double> Rates) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelGroup item, double valueValue)
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

public sealed record FlaggedCostModelGroupAdjuster(Registry Flags, string Flag, double Percent) : ICostModelGroupAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model group steps in order and keeps a trace of each.</summary>
public sealed class CostModelGroupEngine(List<ICostModelGroupAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelGroupAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelGroupEngine DefaultCostModelGroupEngine(Registry flags)
    {
        return new CostModelGroupEngine(
            new List<ICostModelGroupAdjuster>
            {
                new RoundingCostModelGroupAdjuster(0.05),
                new PercentCostModelGroupAdjuster(17.4),
                new CostModelGroupCeilingOffsetAdjuster(1.58),
                new MinimumCostModelGroupAdjuster(5.5),
                new TieredCostModelGroupAdjuster(
                    new List<double> { 2974.0, 8488.0 },
                    new List<double> { 0.98, 0.91 }),
                new FlaggedCostModelGroupAdjuster(flags, "cost-model-group-global", -0.5),
            },
            new List<string>(),
            3.2,
            58107.8);
    }

    public double Evaluate(CostModelGroup item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
