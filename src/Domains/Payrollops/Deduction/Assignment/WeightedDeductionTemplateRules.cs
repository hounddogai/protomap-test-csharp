namespace Acme.Workforce.Payrollops.Deduction.Assignment;

/// <summary>A rule a weighted deduction template must pass. Check returns its problems, or none.</summary>
public interface IWeightedDeductionTemplateRule
{
    List<string> Check(WeightedDeductionTemplate item);
}

public sealed record WeightedDeductionTemplateCodePrefixRule(string Prefix) : IWeightedDeductionTemplateRule
{
    public List<string> Check(WeightedDeductionTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeductionTemplateRequiredLabelRule(string Message) : IWeightedDeductionTemplateRule
{
    public List<string> Check(WeightedDeductionTemplate item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedDeductionTemplateRequiredManagerIdRule(string Message) : IWeightedDeductionTemplateRule
{
    public List<string> Check(WeightedDeductionTemplate item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted deduction template.</summary>
public sealed record WeightedDeductionTemplateValidator(List<IWeightedDeductionTemplateRule> Rules)
{
    public List<string> Validate(WeightedDeductionTemplate item)
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

    public void RequireValid(WeightedDeductionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedDeductionTemplate> items)
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

/// <summary>Functions of weighted deduction template rules.</summary>
public static class WeightedDeductionTemplateRules
{
    public static WeightedDeductionTemplateValidator DefaultWeightedDeductionTemplateValidator()
    {
        return new WeightedDeductionTemplateValidator(
            new List<IWeightedDeductionTemplateRule>
            {
                new WeightedDeductionTemplateCodePrefixRule("X"),
                new WeightedDeductionTemplateRequiredLabelRule("weighted deduction template needs an entry"),
                new WeightedDeductionTemplateRequiredManagerIdRule("weighted deduction template needs an entry"),
            });
    }
}
