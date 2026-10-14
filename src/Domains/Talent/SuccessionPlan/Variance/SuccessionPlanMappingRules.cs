namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>A rule a succession plan mapping must pass. Check returns its problems, or none.</summary>
public interface ISuccessionPlanMappingRule
{
    List<string> Check(SuccessionPlanMapping item);
}

public sealed record SuccessionPlanMappingQuantityRangeRule(int Min, int Max) : ISuccessionPlanMappingRule
{
    public List<string> Check(SuccessionPlanMapping item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanMappingRequiredTitleRule(string Message) : ISuccessionPlanMappingRule
{
    public List<string> Check(SuccessionPlanMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SuccessionPlanMappingTitleLengthRule(int MaxLength) : ISuccessionPlanMappingRule
{
    public List<string> Check(SuccessionPlanMapping item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a succession plan mapping.</summary>
public sealed record SuccessionPlanMappingValidator(List<ISuccessionPlanMappingRule> Rules)
{
    public List<string> Validate(SuccessionPlanMapping item)
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

    public void RequireValid(SuccessionPlanMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SuccessionPlanMapping> items)
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

/// <summary>Functions of succession plan mapping rules.</summary>
public static class SuccessionPlanMappingRules
{
    public static SuccessionPlanMappingValidator DefaultSuccessionPlanMappingValidator()
    {
        return new SuccessionPlanMappingValidator(
            new List<ISuccessionPlanMappingRule>
            {
                new SuccessionPlanMappingQuantityRangeRule(16, 64),
                new SuccessionPlanMappingRequiredTitleRule("succession plan mapping needs an entry"),
                new SuccessionPlanMappingTitleLengthRule(120),
            });
    }
}
