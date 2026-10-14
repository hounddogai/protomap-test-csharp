namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model exception calculation.</summary>
public interface ICostModelExceptionModifier
{
    string Describe();
    double Adjust(CostModelException item, double valueValue);
}

public sealed record MinimumCostModelExceptionAdjuster(double Minimum) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCostModelExceptionAdjuster(double Step) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapCostModelExceptionAdjuster(double Cap) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCostModelExceptionAdjuster(double Percent) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleCostModelExceptionAdjuster(Dictionary<string, double> Factors) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedCostModelExceptionAdjuster(Registry Flags, string Flag, double Percent) : ICostModelExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model exception steps in order and keeps a trace of each.</summary>
public sealed class CostModelExceptionEngine(List<ICostModelExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelExceptionEngine DefaultCostModelExceptionEngine(Registry flags)
    {
        return new CostModelExceptionEngine(
            new List<ICostModelExceptionModifier>
            {
                new MinimumCostModelExceptionAdjuster(39.9),
                new RoundingCostModelExceptionAdjuster(0.5),
                new CapCostModelExceptionAdjuster(18212.0),
                new PercentCostModelExceptionAdjuster(9.3),
                new LocaleCostModelExceptionAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.91, ["north"] = 1.39, ["south"] = 1.19 }),
                new FlaggedCostModelExceptionAdjuster(flags, "cost-model-exception-scheduled", 9.2),
            },
            new List<string>(),
            3.9,
            46347.1);
    }

    public double Price(CostModelException item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
