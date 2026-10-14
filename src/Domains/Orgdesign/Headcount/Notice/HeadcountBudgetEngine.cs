namespace Acme.Workforce.Orgdesign.Headcount.Notice;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount budget calculation.</summary>
public interface IHeadcountBudgetAdjuster
{
    string Describe();
    double Adjust(HeadcountBudget item, double valueValue);
}

public sealed record HeadcountBudgetSequenceOffsetAdjuster(double Weight) : IHeadcountBudgetAdjuster
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(HeadcountBudget item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record RoundingHeadcountBudgetAdjuster(double Step) : IHeadcountBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleHeadcountBudgetAdjuster(Dictionary<string, double> Factors) : IHeadcountBudgetAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(HeadcountBudget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the headcount budget steps in order and keeps a trace of each.</summary>
public sealed class HeadcountBudgetEngine(List<IHeadcountBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountBudgetEngine DefaultHeadcountBudgetEngine()
    {
        return new HeadcountBudgetEngine(
            new List<IHeadcountBudgetAdjuster>
            {
                new HeadcountBudgetSequenceOffsetAdjuster(0.51),
                new RoundingHeadcountBudgetAdjuster(5.0),
                new TitleHeadcountBudgetAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.85, ["online"] = 0.9, ["west"] = 1.28 }),
            },
            new List<string>(),
            0.8,
            40514.7);
    }

    public double Price(HeadcountBudget item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
