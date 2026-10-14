namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched warranty bundle must pass. Check returns its problems, or none.</summary>
public interface IBatchedWarrantyBundleRule
{
    List<string> Check(BatchedWarrantyBundle item);
}

public sealed record BatchedWarrantyBundleChecksumLengthRule(int MaxLength) : IBatchedWarrantyBundleRule
{
    public List<string> Check(BatchedWarrantyBundle item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyBundleAllowedDepartmentRule(List<string> Allowed) : IBatchedWarrantyBundleRule
{
    public List<string> Check(BatchedWarrantyBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record BatchedWarrantyBundleAmountRangeRule(double Min, double Max) : IBatchedWarrantyBundleRule
{
    public List<string> Check(BatchedWarrantyBundle item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BatchedWarrantyBundleNameLengthRule(int MaxLength) : IBatchedWarrantyBundleRule
{
    public List<string> Check(BatchedWarrantyBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched warranty bundle.</summary>
public sealed record BatchedWarrantyBundleValidator(List<IBatchedWarrantyBundleRule> Rules)
{
    public List<string> Validate(BatchedWarrantyBundle item)
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

    public void RequireValid(BatchedWarrantyBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedWarrantyBundle> items)
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

/// <summary>Functions of batched warranty bundle rules.</summary>
public static class BatchedWarrantyBundleRules
{
    public static BatchedWarrantyBundleValidator DefaultBatchedWarrantyBundleValidator()
    {
        return new BatchedWarrantyBundleValidator(
            new List<IBatchedWarrantyBundleRule>
            {
                new BatchedWarrantyBundleChecksumLengthRule(86400),
                new BatchedWarrantyBundleAllowedDepartmentRule(new List<string>()),
                new BatchedWarrantyBundleAmountRangeRule(3.0, 324.8),
                new BatchedWarrantyBundleNameLengthRule(120),
            });
    }
}
