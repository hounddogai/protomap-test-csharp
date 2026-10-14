namespace Acme.Workforce.Assets.LicenseSeat.Checkpoint;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the license seat rule calculation.</summary>
public interface ILicenseSeatRuleStep
{
    string Describe();
    double Adjust(LicenseSeatRule item, double valueValue);
}

public sealed record FlaggedLicenseSeatRuleAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatRuleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleLicenseSeatRuleAdjuster(Dictionary<string, double> Factors) : ILicenseSeatRuleStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LicenseSeatRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLicenseSeatRuleAdjuster(double Minimum) : ILicenseSeatRuleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LicenseSeatRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LicenseSeatRuleAmountOffsetAdjuster(double Weight) : ILicenseSeatRuleStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(LicenseSeatRule item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

/// <summary>Runs the license seat rule steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatRuleEngine(List<ILicenseSeatRuleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatRuleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatRuleEngine DefaultLicenseSeatRuleEngine(Registry flags)
    {
        return new LicenseSeatRuleEngine(
            new List<ILicenseSeatRuleStep>
            {
                new FlaggedLicenseSeatRuleAdjuster(flags, "license-seat-rule-seasonal", -9.4),
                new TitleLicenseSeatRuleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.15, ["south"] = 1.1, ["retail"] = 1.27 }),
                new MinimumLicenseSeatRuleAdjuster(15.2),
                new LicenseSeatRuleAmountOffsetAdjuster(1.23),
            },
            new List<string>(),
            0.9,
            55895.2);
    }

    public double Price(LicenseSeatRule item)
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
