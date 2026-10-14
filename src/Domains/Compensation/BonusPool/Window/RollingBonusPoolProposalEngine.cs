namespace Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>One step of the rolling bonus pool proposal calculation.</summary>
public interface IRollingBonusPoolProposalStep
{
    string Describe();
    double Adjust(RollingBonusPoolProposal item, double valueValue);
}

public sealed record RollingBonusPoolProposalAmountOffsetAdjuster(double Weight) : IRollingBonusPoolProposalStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(RollingBonusPoolProposal item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record PercentRollingBonusPoolProposalAdjuster(double Percent) : IRollingBonusPoolProposalStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingBonusPoolProposal item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record ChannelRollingBonusPoolProposalAdjuster(Dictionary<string, double> Factors) : IRollingBonusPoolProposalStep
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(RollingBonusPoolProposal item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the rolling bonus pool proposal steps in order and keeps a trace of each.</summary>
public sealed class RollingBonusPoolProposalEngine(List<IRollingBonusPoolProposalStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingBonusPoolProposalStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingBonusPoolProposalEngine DefaultRollingBonusPoolProposalEngine()
    {
        return new RollingBonusPoolProposalEngine(
            new List<IRollingBonusPoolProposalStep>
            {
                new RollingBonusPoolProposalAmountOffsetAdjuster(1.47),
                new PercentRollingBonusPoolProposalAdjuster(-10.9),
                new ChannelRollingBonusPoolProposalAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.74, ["north"] = 0.72, ["retail"] = 1.06 }),
            },
            new List<string>(),
            0.3,
            71051.9);
    }

    public double Run(RollingBonusPoolProposal item)
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

    public Dictionary<string, double> EvaluateAll(List<RollingBonusPoolProposal> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
