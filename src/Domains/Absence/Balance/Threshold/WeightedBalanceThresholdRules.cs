namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted balance threshold must pass. Check returns its problems, or none.</summary>
public interface IWeightedBalanceThresholdRule
{
    List<string> Check(WeightedBalanceThreshold item);
}

public sealed record WeightedBalanceThresholdLabelLengthRule(int MaxLength) : IWeightedBalanceThresholdRule
{
    public List<string> Check(WeightedBalanceThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceThresholdRegionLengthRule(int MaxLength) : IWeightedBalanceThresholdRule
{
    public List<string> Check(WeightedBalanceThreshold item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceThresholdRequiredRegionRule(string Message) : IWeightedBalanceThresholdRule
{
    public List<string> Check(WeightedBalanceThreshold item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBalanceThresholdAllowedCodeRule(List<string> Allowed) : IWeightedBalanceThresholdRule
{
    public List<string> Check(WeightedBalanceThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is duplicated" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted balance threshold.</summary>
public sealed record WeightedBalanceThresholdValidator(List<IWeightedBalanceThresholdRule> Rules)
{
    public List<string> Validate(WeightedBalanceThreshold item)
    {
        var problems = new List<string>();
        foreach (var rule in Rules)
        {
            foreach (var problem in rule.Check(item))
            {
                problems.Add(problem);
            }
        }
        return problems;
    }

    public void RequireValid(WeightedBalanceThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBalanceThreshold> items)
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

/// <summary>Functions of weighted balance threshold rules.</summary>
public static class WeightedBalanceThresholdRules
{
    public static WeightedBalanceThresholdValidator DefaultWeightedBalanceThresholdValidator()
    {
        return new WeightedBalanceThresholdValidator(
            new List<IWeightedBalanceThresholdRule>
            {
                new WeightedBalanceThresholdLabelLengthRule(80),
                new WeightedBalanceThresholdRegionLengthRule(80),
                new WeightedBalanceThresholdRequiredRegionRule("weighted balance threshold needs an entry"),
                new WeightedBalanceThresholdAllowedCodeRule(new List<string> { "batched", "pending", "strict" }),
            });
    }
}
