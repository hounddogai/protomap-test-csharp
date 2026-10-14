namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default certification budget calculation.</summary>
public interface IDefaultCertificationBudgetAdjuster
{
    string Describe();
    double Adjust(DefaultCertificationBudget item, double valueValue);
}

public sealed record TieredDefaultCertificationBudgetAdjuster(List<double> Bounds, List<double> Rates) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
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

public sealed record RoundingDefaultCertificationBudgetAdjuster(double Step) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedDefaultCertificationBudgetAdjuster(Registry Flags, string Flag, double Percent) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentDefaultCertificationBudgetAdjuster(double Percent) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDefaultCertificationBudgetAdjuster(double Minimum) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DefaultCertificationBudgetRatioOffsetAdjuster(double Weight) : IDefaultCertificationBudgetAdjuster
{
    public string Describe()
    {
        return $"plus ratio times {Weight}";
    }

    public double Adjust(DefaultCertificationBudget item, double valueValue)
    {
        return valueValue + item.Ratio * Weight;
    }
}

/// <summary>Runs the default certification budget steps in order and keeps a trace of each.</summary>
public sealed class DefaultCertificationBudgetEngine(List<IDefaultCertificationBudgetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultCertificationBudgetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultCertificationBudgetEngine DefaultDefaultCertificationBudgetEngine(Registry flags)
    {
        return new DefaultCertificationBudgetEngine(
            new List<IDefaultCertificationBudgetAdjuster>
            {
                new TieredDefaultCertificationBudgetAdjuster(
                    new List<double> { 1950.0, 6804.0 },
                    new List<double> { 0.85, 1.02 }),
                new RoundingDefaultCertificationBudgetAdjuster(0.05),
                new FlaggedDefaultCertificationBudgetAdjuster(flags, "default-certification-budget-weighted", 5.5),
                new PercentDefaultCertificationBudgetAdjuster(-10.3),
                new MinimumDefaultCertificationBudgetAdjuster(20.9),
                new DefaultCertificationBudgetRatioOffsetAdjuster(0.21),
            },
            new List<string>(),
            2.9,
            71112.5);
    }

    public double Price(DefaultCertificationBudget item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultCertificationBudget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
