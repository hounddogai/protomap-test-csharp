namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional training due request calculation.</summary>
public interface IRegionalTrainingDueRequestAdjuster
{
    string Describe();
    double Adjust(RegionalTrainingDueRequest item, double valueValue);
}

public sealed record RegionalTrainingDueRequestWeightOffsetAdjuster(double Weight) : IRegionalTrainingDueRequestAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RegionalTrainingDueRequest item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapRegionalTrainingDueRequestAdjuster(double Cap) : IRegionalTrainingDueRequestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalTrainingDueRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRegionalTrainingDueRequestAdjuster(double Minimum) : IRegionalTrainingDueRequestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalTrainingDueRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentRegionalTrainingDueRequestAdjuster(double Percent) : IRegionalTrainingDueRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalTrainingDueRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalTrainingDueRequestAdjuster(List<double> Bounds, List<double> Rates) : IRegionalTrainingDueRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalTrainingDueRequest item, double valueValue)
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

/// <summary>Runs the regional training due request steps in order and keeps a trace of each.</summary>
public sealed class RegionalTrainingDueRequestEngine(List<IRegionalTrainingDueRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalTrainingDueRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalTrainingDueRequestEngine DefaultRegionalTrainingDueRequestEngine()
    {
        return new RegionalTrainingDueRequestEngine(
            new List<IRegionalTrainingDueRequestAdjuster>
            {
                new RegionalTrainingDueRequestWeightOffsetAdjuster(1.64),
                new CapRegionalTrainingDueRequestAdjuster(12897.0),
                new MinimumRegionalTrainingDueRequestAdjuster(44.3),
                new PercentRegionalTrainingDueRequestAdjuster(-9.9),
                new TieredRegionalTrainingDueRequestAdjuster(
                    new List<double> { 1288.0, 2258.0, 5528.0, 7229.0 },
                    new List<double> { 0.94, 1.01, 1.19, 1.19 }),
            },
            new List<string>(),
            1.8,
            36901.8);
    }

    public double Run(RegionalTrainingDueRequest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalTrainingDueRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
