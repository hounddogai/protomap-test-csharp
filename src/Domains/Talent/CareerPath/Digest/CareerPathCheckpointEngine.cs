namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the career path checkpoint calculation.</summary>
public interface ICareerPathCheckpointFactor
{
    string Describe();
    double Adjust(CareerPathCheckpoint item, double valueValue);
}

public sealed record RoundingCareerPathCheckpointAdjuster(double Step) : ICareerPathCheckpointFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CareerPathCheckpoint item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumCareerPathCheckpointAdjuster(double Minimum) : ICareerPathCheckpointFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CareerPathCheckpoint item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapCareerPathCheckpointAdjuster(double Cap) : ICareerPathCheckpointFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CareerPathCheckpoint item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCareerPathCheckpointAdjuster(Registry Flags, string Flag, double Percent) : ICareerPathCheckpointFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CareerPathCheckpoint item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the career path checkpoint steps in order and keeps a trace of each.</summary>
public sealed class CareerPathCheckpointEngine(List<ICareerPathCheckpointFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICareerPathCheckpointFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CareerPathCheckpointEngine DefaultCareerPathCheckpointEngine(Registry flags)
    {
        return new CareerPathCheckpointEngine(
            new List<ICareerPathCheckpointFactor>
            {
                new RoundingCareerPathCheckpointAdjuster(1.0),
                new MinimumCareerPathCheckpointAdjuster(8.8),
                new CapCareerPathCheckpointAdjuster(17669.0),
                new FlaggedCareerPathCheckpointAdjuster(flags, "career-path-checkpoint-seasonal", -9.1),
            },
            new List<string>(),
            0.3,
            13531.8);
    }

    public double Run(CareerPathCheckpoint item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<CareerPathCheckpoint> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
