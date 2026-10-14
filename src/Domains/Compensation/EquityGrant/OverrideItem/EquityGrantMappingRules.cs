namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>A rule a equity grant mapping must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantMappingRule
{
    List<string> Check(EquityGrantMapping item);
}

public sealed record EquityGrantMappingCodePrefixRule(string Prefix) : IEquityGrantMappingRule
{
    public List<string> Check(EquityGrantMapping item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMappingBudgetRangeRule(double Min, double Max) : IEquityGrantMappingRule
{
    public List<string> Check(EquityGrantMapping item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantMappingRequiredNameRule(string Message) : IEquityGrantMappingRule
{
    public List<string> Check(EquityGrantMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantMappingRequiredCodeRule(string Message) : IEquityGrantMappingRule
{
    public List<string> Check(EquityGrantMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant mapping.</summary>
public sealed record EquityGrantMappingValidator(List<IEquityGrantMappingRule> Rules)
{
    public List<string> Validate(EquityGrantMapping item)
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

    public void RequireValid(EquityGrantMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantMapping> items)
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

/// <summary>Functions of equity grant mapping rules.</summary>
public static class EquityGrantMappingRules
{
    public static EquityGrantMappingValidator DefaultEquityGrantMappingValidator()
    {
        return new EquityGrantMappingValidator(
            new List<IEquityGrantMappingRule>
            {
                new EquityGrantMappingCodePrefixRule("ACM"),
                new EquityGrantMappingBudgetRangeRule(3.3, 4871.7),
                new EquityGrantMappingRequiredNameRule("equity grant mapping needs an entry"),
                new EquityGrantMappingRequiredCodeRule("equity grant mapping needs this field"),
            });
    }
}
