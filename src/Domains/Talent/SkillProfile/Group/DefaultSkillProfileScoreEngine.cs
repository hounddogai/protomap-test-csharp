namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default skill profile score calculation.</summary>
public interface IDefaultSkillProfileScoreFactor
{
    string Describe();
    double Adjust(DefaultSkillProfileScore item, double valueValue);
}

public sealed record RoundingDefaultSkillProfileScoreAdjuster(double Step) : IDefaultSkillProfileScoreFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultSkillProfileScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredDefaultSkillProfileScoreAdjuster(List<double> Bounds, List<double> Rates) : IDefaultSkillProfileScoreFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultSkillProfileScore item, double valueValue)
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

public sealed record CapDefaultSkillProfileScoreAdjuster(double Cap) : IDefaultSkillProfileScoreFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultSkillProfileScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LabelDefaultSkillProfileScoreAdjuster(Dictionary<string, double> Factors) : IDefaultSkillProfileScoreFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(DefaultSkillProfileScore item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the default skill profile score steps in order and keeps a trace of each.</summary>
public sealed class DefaultSkillProfileScoreEngine(List<IDefaultSkillProfileScoreFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultSkillProfileScoreFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultSkillProfileScoreEngine DefaultDefaultSkillProfileScoreEngine()
    {
        return new DefaultSkillProfileScoreEngine(
            new List<IDefaultSkillProfileScoreFactor>
            {
                new RoundingDefaultSkillProfileScoreAdjuster(1.0),
                new TieredDefaultSkillProfileScoreAdjuster(
                    new List<double> { 918.0, 3177.0, 3345.0 },
                    new List<double> { 0.89, 0.95, 1.01 }),
                new CapDefaultSkillProfileScoreAdjuster(33141.0),
                new LabelDefaultSkillProfileScoreAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.38, ["central"] = 1.06 }),
            },
            new List<string>(),
            2.4,
            35968.9);
    }

    public double Price(DefaultSkillProfileScore item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultSkillProfileScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
