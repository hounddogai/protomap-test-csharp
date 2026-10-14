namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile entry calculation.</summary>
public interface ISkillProfileEntryFactor
{
    string Describe();
    double Adjust(SkillProfileEntry item, double valueValue);
}

public sealed record TieredSkillProfileEntryAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileEntryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileEntry item, double valueValue)
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

public sealed record MinimumSkillProfileEntryAdjuster(double Minimum) : ISkillProfileEntryFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSkillProfileEntryAdjuster(double Cap) : ISkillProfileEntryFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SkillProfileEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingSkillProfileEntryAdjuster(double Step) : ISkillProfileEntryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the skill profile entry steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileEntryEngine(List<ISkillProfileEntryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileEntryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileEntryEngine DefaultSkillProfileEntryEngine()
    {
        return new SkillProfileEntryEngine(
            new List<ISkillProfileEntryFactor>
            {
                new TieredSkillProfileEntryAdjuster(
                    new List<double> { 2264.0, 6372.0, 9971.0 },
                    new List<double> { 1.14, 0.85, 0.9 }),
                new MinimumSkillProfileEntryAdjuster(49.1),
                new CapSkillProfileEntryAdjuster(38561.0),
                new RoundingSkillProfileEntryAdjuster(0.1),
            },
            new List<string>(),
            3.6,
            37410.5);
    }

    public double Price(SkillProfileEntry item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
