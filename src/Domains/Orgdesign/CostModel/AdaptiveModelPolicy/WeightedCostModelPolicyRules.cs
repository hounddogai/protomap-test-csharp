namespace Acme.Workforce.Orgdesign.CostModel.AdaptiveModelPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted cost model policy must pass. Check returns its problems, or none.</summary>
public interface IWeightedCostModelPolicyRule
{
    List<string> Check(WeightedCostModelPolicy item);
}

public sealed record WeightedCostModelPolicyCodeLengthRule(int MaxLength) : IWeightedCostModelPolicyRule
{
    public List<string> Check(WeightedCostModelPolicy item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelPolicyRequiredCodeRule(string Message) : IWeightedCostModelPolicyRule
{
    public List<string> Check(WeightedCostModelPolicy item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedCostModelPolicyAllowedDescriptionRule(List<string> Allowed) : IWeightedCostModelPolicyRule
{
    public List<string> Check(WeightedCostModelPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Description)))
        {
            return new List<string> { $"description {item.Description} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted cost model policy.</summary>
public sealed record WeightedCostModelPolicyValidator(List<IWeightedCostModelPolicyRule> Rules)
{
    public List<string> Validate(WeightedCostModelPolicy item)
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

    public void RequireValid(WeightedCostModelPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedCostModelPolicy> items)
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

/// <summary>Functions of weighted cost model policy rules.</summary>
public static class WeightedCostModelPolicyRules
{
    public static WeightedCostModelPolicyValidator DefaultWeightedCostModelPolicyValidator()
    {
        return new WeightedCostModelPolicyValidator(
            new List<IWeightedCostModelPolicyRule>
            {
                new WeightedCostModelPolicyCodeLengthRule(86400),
                new WeightedCostModelPolicyRequiredCodeRule("weighted cost model policy needs a value"),
                new WeightedCostModelPolicyAllowedDescriptionRule(new List<string>()),
            });
    }
}
