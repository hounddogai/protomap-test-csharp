namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the skill profile bundle calculation.</summary>
public interface ISkillProfileBundleFactor
{
    string Describe();
    double Adjust(SkillProfileBundle item, double valueValue);
}

public sealed record LabelSkillProfileBundleAdjuster(Dictionary<string, double> Factors) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingSkillProfileBundleAdjuster(double Step) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSkillProfileBundleAdjuster(double Minimum) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentSkillProfileBundleAdjuster(double Percent) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSkillProfileBundleAdjuster(List<double> Bounds, List<double> Rates) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
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

public sealed record SkillProfileBundleMarginOffsetAdjuster(double Weight) : ISkillProfileBundleFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(SkillProfileBundle item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

/// <summary>Runs the skill profile bundle steps in order and keeps a trace of each.</summary>
public sealed class SkillProfileBundleEngine(List<ISkillProfileBundleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISkillProfileBundleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SkillProfileBundleEngine DefaultSkillProfileBundleEngine()
    {
        return new SkillProfileBundleEngine(
            new List<ISkillProfileBundleFactor>
            {
                new LabelSkillProfileBundleAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.38, ["west"] = 1.07 }),
                new RoundingSkillProfileBundleAdjuster(1.0),
                new MinimumSkillProfileBundleAdjuster(2.2),
                new PercentSkillProfileBundleAdjuster(-24.0),
                new TieredSkillProfileBundleAdjuster(
                    new List<double> { 1148.0, 3361.0, 4642.0, 9254.0 },
                    new List<double> { 0.85, 1.14, 1.02, 0.8 }),
                new SkillProfileBundleMarginOffsetAdjuster(1.86),
            },
            new List<string>(),
            0.4,
            6808.3);
    }

    public double Price(SkillProfileBundle item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SkillProfileBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
