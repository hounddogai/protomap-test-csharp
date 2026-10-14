namespace Acme.Workforce.Assets.Assignment.Checkpoint;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the assignment score calculation.</summary>
public interface IAssignmentScoreFactor
{
    string Describe();
    double Adjust(AssignmentScore item, double valueValue);
}

public sealed record RoundingAssignmentScoreAdjuster(double Step) : IAssignmentScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AssignmentScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record AssignmentScoreFloorValueOffsetAdjuster(double Weight) : IAssignmentScoreFactor
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(AssignmentScore item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

public sealed record TieredAssignmentScoreAdjuster(List<double> Bounds, List<double> Rates) : IAssignmentScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AssignmentScore item, double valueValue)
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

public sealed record CapAssignmentScoreAdjuster(double Cap) : IAssignmentScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AssignmentScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the assignment score steps in order and keeps a trace of each.</summary>
public sealed class AssignmentScoreEngine(List<IAssignmentScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAssignmentScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AssignmentScoreEngine DefaultAssignmentScoreEngine()
    {
        return new AssignmentScoreEngine(
            new List<IAssignmentScoreFactor>
            {
                new RoundingAssignmentScoreAdjuster(1.0),
                new AssignmentScoreFloorValueOffsetAdjuster(1.78),
                new TieredAssignmentScoreAdjuster(
                    new List<double> { 4650.0, 5003.0 },
                    new List<double> { 0.93, 0.9 }),
                new CapAssignmentScoreAdjuster(15744.0),
            },
            new List<string>(),
            0.8,
            94003.1);
    }

    public double Compute(AssignmentScore item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<AssignmentScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
