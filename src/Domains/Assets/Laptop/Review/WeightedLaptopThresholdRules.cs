namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted laptop threshold must pass. Check returns its problems, or none.</summary>
public interface IWeightedLaptopThresholdRule
{
    List<string> Check(WeightedLaptopThreshold item);
}

public sealed record WeightedLaptopThresholdAllowedTeamRule(List<string> Allowed) : IWeightedLaptopThresholdRule
{
    public List<string> Check(WeightedLaptopThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is unknown" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopThresholdCodePrefixRule(string Prefix) : IWeightedLaptopThresholdRule
{
    public List<string> Check(WeightedLaptopThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedLaptopThresholdRequiredCodeRule(string Message) : IWeightedLaptopThresholdRule
{
    public List<string> Check(WeightedLaptopThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted laptop threshold.</summary>
public sealed record WeightedLaptopThresholdValidator(List<IWeightedLaptopThresholdRule> Rules)
{
    public List<string> Validate(WeightedLaptopThreshold item)
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

    public void RequireValid(WeightedLaptopThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedLaptopThreshold> items)
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

/// <summary>Functions of weighted laptop threshold rules.</summary>
public static class WeightedLaptopThresholdRules
{
    public static WeightedLaptopThresholdValidator DefaultWeightedLaptopThresholdValidator()
    {
        return new WeightedLaptopThresholdValidator(
            new List<IWeightedLaptopThresholdRule>
            {
                new WeightedLaptopThresholdAllowedTeamRule(new List<string> { "seasonal" }),
                new WeightedLaptopThresholdCodePrefixRule("OPS"),
                new WeightedLaptopThresholdRequiredCodeRule("weighted laptop threshold needs this field"),
            });
    }
}
