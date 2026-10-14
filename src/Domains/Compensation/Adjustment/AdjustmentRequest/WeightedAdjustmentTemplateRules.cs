namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted adjustment template must pass. Check returns its problems, or none.</summary>
public interface IWeightedAdjustmentTemplateRule
{
    List<string> Check(WeightedAdjustmentTemplate item);
}

public sealed record WeightedAdjustmentTemplateRequiredDepartmentRule(string Message) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentTemplateSourceLengthRule(int MaxLength) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentTemplateRequiredLabelRule(string Message) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentTemplateRequiredCodeRule(string Message) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentTemplateAllowedManagerIdRule(List<string> Allowed) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ManagerId)))
        {
            return new List<string> { $"manager id {item.ManagerId} is too long" };
        }
        return new List<string>();
    }
}

public sealed record WeightedAdjustmentTemplateCodePrefixRule(string Prefix) : IWeightedAdjustmentTemplateRule
{
    public List<string> Check(WeightedAdjustmentTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted adjustment template.</summary>
public sealed record WeightedAdjustmentTemplateValidator(List<IWeightedAdjustmentTemplateRule> Rules)
{
    public List<string> Validate(WeightedAdjustmentTemplate item)
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

    public void RequireValid(WeightedAdjustmentTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedAdjustmentTemplate> items)
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

/// <summary>Functions of weighted adjustment template rules.</summary>
public static class WeightedAdjustmentTemplateRules
{
    public static WeightedAdjustmentTemplateValidator DefaultWeightedAdjustmentTemplateValidator()
    {
        return new WeightedAdjustmentTemplateValidator(
            new List<IWeightedAdjustmentTemplateRule>
            {
                new WeightedAdjustmentTemplateRequiredDepartmentRule("weighted adjustment template needs an entry"),
                new WeightedAdjustmentTemplateSourceLengthRule(64),
                new WeightedAdjustmentTemplateRequiredLabelRule("weighted adjustment template needs this field"),
                new WeightedAdjustmentTemplateRequiredCodeRule("weighted adjustment template needs an entry"),
                new WeightedAdjustmentTemplateAllowedManagerIdRule(new List<string>()),
                new WeightedAdjustmentTemplateCodePrefixRule("ACM"),
            });
    }
}
