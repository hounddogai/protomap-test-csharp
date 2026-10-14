namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a mobility rollup must pass. Check returns its problems, or none.</summary>
public interface IMobilityRollupRule
{
    List<string> Check(MobilityRollup item);
}

public sealed record MobilityRollupQuantityRangeRule(int Min, int Max) : IMobilityRollupRule
{
    public List<string> Check(MobilityRollup item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record MobilityRollupAllowedSourceRule(List<string> Allowed) : IMobilityRollupRule
{
    public List<string> Check(MobilityRollup item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record MobilityRollupRequiredSourceRule(string Message) : IMobilityRollupRule
{
    public List<string> Check(MobilityRollup item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record MobilityRollupMaximumRangeRule(double Min, double Max) : IMobilityRollupRule
{
    public List<string> Check(MobilityRollup item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a mobility rollup.</summary>
public sealed record MobilityRollupValidator(List<IMobilityRollupRule> Rules)
{
    public List<string> Validate(MobilityRollup item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            var found = rule.Check(item);
            if (found.Count == 0)
            {
                continue;
            }
            foreach (var problem in found)
            {
                problems.Add($"{item.Code}: {problem}");
            }
        }
        return problems;
    }

    public void RequireValid(MobilityRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<MobilityRollup> items)
    {
        var report = new Dictionary<string, List<string>>();
        foreach (var item in items)
        {
            var problems = Validate(item);
            if (problems.Count > 0)
            {
                report[item.Code] = problems;
            }
        }
        return report;
    }
}

/// <summary>Functions of mobility rollup rules.</summary>
public static class MobilityRollupRules
{
    public static MobilityRollupValidator DefaultMobilityRollupValidator()
    {
        return new MobilityRollupValidator(
            new List<IMobilityRollupRule>
            {
                new MobilityRollupQuantityRangeRule(64, 86400),
                new MobilityRollupAllowedSourceRule(new List<string>()),
                new MobilityRollupRequiredSourceRule("mobility rollup needs this field"),
                new MobilityRollupMaximumRangeRule(8.6, 222.3),
            });
    }
}
