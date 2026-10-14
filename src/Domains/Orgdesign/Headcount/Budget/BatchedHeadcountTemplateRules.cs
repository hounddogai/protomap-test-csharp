namespace Acme.Workforce.Orgdesign.Headcount.Budget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched headcount template must pass. Check returns its problems, or none.</summary>
public interface IBatchedHeadcountTemplateRule
{
    List<string> Check(BatchedHeadcountTemplate item);
}

public sealed record BatchedHeadcountTemplateQuantityRangeRule(int Min, int Max) : IBatchedHeadcountTemplateRule
{
    public List<string> Check(BatchedHeadcountTemplate item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedHeadcountTemplateAllowedNameRule(List<string> Allowed) : IBatchedHeadcountTemplateRule
{
    public List<string> Check(BatchedHeadcountTemplate item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is missing" };
        }
        return new List<string>();
    }
}

public sealed record BatchedHeadcountTemplateSalaryBandLengthRule(int MaxLength) : IBatchedHeadcountTemplateRule
{
    public List<string> Check(BatchedHeadcountTemplate item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedHeadcountTemplateRequiredTimezoneRule(string Message) : IBatchedHeadcountTemplateRule
{
    public List<string> Check(BatchedHeadcountTemplate item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched headcount template.</summary>
public sealed record BatchedHeadcountTemplateValidator(List<IBatchedHeadcountTemplateRule> Rules)
{
    public List<string> Validate(BatchedHeadcountTemplate item)
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

    public void RequireValid(BatchedHeadcountTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedHeadcountTemplate> items)
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

/// <summary>Functions of batched headcount template rules.</summary>
public static class BatchedHeadcountTemplateRules
{
    public static BatchedHeadcountTemplateValidator DefaultBatchedHeadcountTemplateValidator()
    {
        return new BatchedHeadcountTemplateValidator(
            new List<IBatchedHeadcountTemplateRule>
            {
                new BatchedHeadcountTemplateQuantityRangeRule(120, 255),
                new BatchedHeadcountTemplateAllowedNameRule(new List<string>()),
                new BatchedHeadcountTemplateSalaryBandLengthRule(16),
                new BatchedHeadcountTemplateRequiredTimezoneRule("batched headcount template needs a value"),
            });
    }
}
