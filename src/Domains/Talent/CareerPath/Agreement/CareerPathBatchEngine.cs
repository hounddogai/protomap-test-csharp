namespace Acme.Workforce.Talent.CareerPath.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path batch calculation.</summary>
public interface ICareerPathBatchFactor
{
    string Describe();
    double Adjust(CareerPathBatch item, double valueValue);
}

public sealed record CapCareerPathBatchAdjuster(double Cap) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCareerPathBatchAdjuster(double Step) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CategoryCareerPathBatchAdjuster(Dictionary<string, double> Factors) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCareerPathBatchAdjuster(double Percent) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CareerPathBatchMinimumOffsetAdjuster(double Weight) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumCareerPathBatchAdjuster(double Minimum) : ICareerPathBatchFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathBatch item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the career path batch steps in order and keeps a trace of each.</summary>
public sealed class CareerPathBatchEngine(List<ICareerPathBatchFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathBatchFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathBatchEngine DefaultCareerPathBatchEngine()
    {
        return new CareerPathBatchEngine(
            new List<ICareerPathBatchFactor>
            {
                new CapCareerPathBatchAdjuster(35771.0),
                new RoundingCareerPathBatchAdjuster(0.1),
                new CategoryCareerPathBatchAdjuster(new Dictionary<string, double> { ["retail"] = 1.26 }),
                new PercentCareerPathBatchAdjuster(10.3),
                new CareerPathBatchMinimumOffsetAdjuster(1.16),
                new MinimumCareerPathBatchAdjuster(36.9),
            },
            new List<string>(),
            3.9,
            84129.2);
    }

    public double Evaluate(CareerPathBatch item)
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
