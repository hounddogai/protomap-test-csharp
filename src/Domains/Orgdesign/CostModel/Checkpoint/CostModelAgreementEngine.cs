namespace Acme.Workforce.Orgdesign.CostModel.Checkpoint;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the cost model agreement calculation.</summary>
public interface ICostModelAgreementAdjuster
{
    string Describe();
    double Adjust(CostModelAgreement item, double valueValue);
}

public sealed record MinimumCostModelAgreementAdjuster(double Minimum) : ICostModelAgreementAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CostModelAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CostModelAgreementAmountOffsetAdjuster(double Weight) : ICostModelAgreementAdjuster
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(CostModelAgreement item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record TieredCostModelAgreementAdjuster(List<double> Bounds, List<double> Rates) : ICostModelAgreementAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CostModelAgreement item, double valueValue)
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

public sealed record RoundingCostModelAgreementAdjuster(double Step) : ICostModelAgreementAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CostModelAgreement item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCostModelAgreementAdjuster(Registry Flags, string Flag, double Percent) : ICostModelAgreementAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CostModelAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the cost model agreement steps in order and keeps a trace of each.</summary>
public sealed class CostModelAgreementEngine(List<ICostModelAgreementAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICostModelAgreementAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CostModelAgreementEngine DefaultCostModelAgreementEngine(Registry flags)
    {
        return new CostModelAgreementEngine(
            new List<ICostModelAgreementAdjuster>
            {
                new MinimumCostModelAgreementAdjuster(3.5),
                new CostModelAgreementAmountOffsetAdjuster(1.79),
                new TieredCostModelAgreementAdjuster(
                    new List<double> { 1406.0, 7860.0, 8021.0, 9618.0 },
                    new List<double> { 0.86, 0.96, 1.08, 0.97 }),
                new RoundingCostModelAgreementAdjuster(1.0),
                new FlaggedCostModelAgreementAdjuster(flags, "cost-model-agreement-scheduled", 7.2),
            },
            new List<string>(),
            0.6,
            16057.8);
    }

    public double Run(CostModelAgreement item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CostModelAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
