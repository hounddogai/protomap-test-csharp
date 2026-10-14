namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched vendor override must pass. Check returns its problems, or none.</summary>
public interface IBatchedVendorOverrideRule
{
    List<string> Check(BatchedVendorOverride item);
}

public sealed record BatchedVendorOverrideRateRangeRule(double Min, double Max) : IBatchedVendorOverrideRule
{
    public List<string> Check(BatchedVendorOverride item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorOverrideRequiredNameRule(string Message) : IBatchedVendorOverrideRule
{
    public List<string> Check(BatchedVendorOverride item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorOverrideNameLengthRule(int MaxLength) : IBatchedVendorOverrideRule
{
    public List<string> Check(BatchedVendorOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorOverrideAllowedTitleRule(List<string> Allowed) : IBatchedVendorOverrideRule
{
    public List<string> Check(BatchedVendorOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched vendor override.</summary>
public sealed record BatchedVendorOverrideValidator(List<IBatchedVendorOverrideRule> Rules)
{
    public List<string> Validate(BatchedVendorOverride item)
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

    public void RequireValid(BatchedVendorOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedVendorOverride> items)
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

/// <summary>Functions of batched vendor override rules.</summary>
public static class BatchedVendorOverrideRules
{
    public static BatchedVendorOverrideValidator DefaultBatchedVendorOverrideValidator()
    {
        return new BatchedVendorOverrideValidator(
            new List<IBatchedVendorOverrideRule>
            {
                new BatchedVendorOverrideRateRangeRule(5.7, 4224.3),
                new BatchedVendorOverrideRequiredNameRule("batched vendor override needs an entry"),
                new BatchedVendorOverrideNameLengthRule(86400),
                new BatchedVendorOverrideAllowedTitleRule(new List<string> { "regional" }),
            });
    }
}
