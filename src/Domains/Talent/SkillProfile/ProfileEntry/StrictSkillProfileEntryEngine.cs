namespace Acme.Workforce.Talent.SkillProfile.ProfileEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict skill profile entry calculation.</summary>
public interface IStrictSkillProfileEntryFactor
{
    string Describe();
    double Adjust(StrictSkillProfileEntry item, double valueValue);
}

public sealed record CapStrictSkillProfileEntryAdjuster(double Cap) : IStrictSkillProfileEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictSkillProfileEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record StrictSkillProfileEntryFteOffsetAdjuster(double Weight) : IStrictSkillProfileEntryFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(StrictSkillProfileEntry item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record MinimumStrictSkillProfileEntryAdjuster(double Minimum) : IStrictSkillProfileEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictSkillProfileEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TeamStrictSkillProfileEntryAdjuster(Dictionary<string, double> Factors) : IStrictSkillProfileEntryFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(StrictSkillProfileEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict skill profile entry steps in order and keeps a trace of each.</summary>
public sealed class StrictSkillProfileEntryEngine(List<IStrictSkillProfileEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictSkillProfileEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictSkillProfileEntryEngine DefaultStrictSkillProfileEntryEngine()
    {
        return new StrictSkillProfileEntryEngine(
            new List<IStrictSkillProfileEntryFactor>
            {
                new CapStrictSkillProfileEntryAdjuster(6213.0),
                new StrictSkillProfileEntryFteOffsetAdjuster(0.01),
                new MinimumStrictSkillProfileEntryAdjuster(45.5),
                new TeamStrictSkillProfileEntryAdjuster(new Dictionary<string, double> { ["south"] = 0.82 }),
            },
            new List<string>(),
            3.5,
            32445.2);
    }

    public double Evaluate(StrictSkillProfileEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<StrictSkillProfileEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
