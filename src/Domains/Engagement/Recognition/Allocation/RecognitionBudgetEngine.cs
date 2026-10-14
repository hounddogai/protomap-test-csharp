namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the recognition budget calculation.</summary>
public interface IRecognitionBudgetAdjuster
{
    string Describe();
    double Adjust(RecognitionBudget item, double valueValue);
}

public sealed record UnitRecognitionBudgetAdjuster(Dictionary<string, double> Factors) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RecognitionBudgetVersionOffsetAdjuster(double Weight) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record MinimumRecognitionBudgetAdjuster(double Minimum) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRecognitionBudgetAdjuster(double Step) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRecognitionBudgetAdjuster(double Percent) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRecognitionBudgetAdjuster(List<double> Bounds, List<double> Rates) : IRecognitionBudgetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RecognitionBudget item, double valueValue)
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

/// <summary>Runs the recognition budget steps in order and keeps a trace of each.</summary>
public sealed class RecognitionBudgetEngine(List<IRecognitionBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRecognitionBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RecognitionBudgetEngine DefaultRecognitionBudgetEngine()
    {
        return new RecognitionBudgetEngine(
            new List<IRecognitionBudgetAdjuster>
            {
                new UnitRecognitionBudgetAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.32, ["south"] = 0.92 }),
                new RecognitionBudgetVersionOffsetAdjuster(0.5),
                new MinimumRecognitionBudgetAdjuster(48.6),
                new RoundingRecognitionBudgetAdjuster(5.0),
                new PercentRecognitionBudgetAdjuster(20.4),
                new TieredRecognitionBudgetAdjuster(
                    new List<double> { 590.0, 7101.0, 9007.0, 9450.0 },
                    new List<double> { 1.2, 1.17, 1.11, 0.96 }),
            },
            new List<string>(),
            1.5,
            71245.2);
    }

    public double Evaluate(RecognitionBudget item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RecognitionBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
