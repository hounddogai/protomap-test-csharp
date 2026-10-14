namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>A rule a batched vendor template must pass. Check returns its problems, or none.</summary>
public interface IBatchedVendorTemplateRule
{
    List<string> Check(BatchedVendorTemplate item);
}

public sealed record BatchedVendorTemplateRequiredCodeRule(string Message) : IBatchedVendorTemplateRule
{
    public List<string> Check(BatchedVendorTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorTemplateTitleLengthRule(int MaxLength) : IBatchedVendorTemplateRule
{
    public List<string> Check(BatchedVendorTemplate item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorTemplateTagsLimitRule(int MaxCount, List<string> Banned) : IBatchedVendorTemplateRule
{
    public List<string> Check(BatchedVendorTemplate item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record BatchedVendorTemplateRequiredSalaryBandRule(string Message) : IBatchedVendorTemplateRule
{
    public List<string> Check(BatchedVendorTemplate item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched vendor template.</summary>
public sealed record BatchedVendorTemplateValidator(List<IBatchedVendorTemplateRule> Rules)
{
    public List<string> Validate(BatchedVendorTemplate item)
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

    public void RequireValid(BatchedVendorTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedVendorTemplate> items)
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

/// <summary>Functions of batched vendor template rules.</summary>
public static class BatchedVendorTemplateRules
{
    public static BatchedVendorTemplateValidator DefaultBatchedVendorTemplateValidator()
    {
        return new BatchedVendorTemplateValidator(
            new List<IBatchedVendorTemplateRule>
            {
                new BatchedVendorTemplateRequiredCodeRule("batched vendor template needs this field"),
                new BatchedVendorTemplateTitleLengthRule(86400),
                new BatchedVendorTemplateTagsLimitRule(120, new List<string> { "rolling", "legacy", "seasonal" }),
                new BatchedVendorTemplateRequiredSalaryBandRule("batched vendor template needs this field"),
            });
    }
}
