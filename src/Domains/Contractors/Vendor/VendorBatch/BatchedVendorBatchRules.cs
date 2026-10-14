namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>A rule a batched vendor batch must pass. Check returns its problems, or none.</summary>
public interface IBatchedVendorBatchRule
{
    List<string> Check(BatchedVendorBatch item);
}

public sealed record BatchedVendorBatchLabelLengthRule(int MaxLength) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorBatchWeightRangeRule(double Min, double Max) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorBatchBadgeLengthRule(int MaxLength) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorBatchGradeLengthRule(int MaxLength) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorBatchCodePrefixRule(string Prefix) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedVendorBatchRequiredCodeRule(string Message) : IBatchedVendorBatchRule
{
    public List<string> Check(BatchedVendorBatch item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched vendor batch.</summary>
public sealed record BatchedVendorBatchValidator(List<IBatchedVendorBatchRule> Rules)
{
    public List<string> Validate(BatchedVendorBatch item)
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

    public void RequireValid(BatchedVendorBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedVendorBatch> items)
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

/// <summary>Functions of batched vendor batch rules.</summary>
public static class BatchedVendorBatchRules
{
    public static BatchedVendorBatchValidator DefaultBatchedVendorBatchValidator()
    {
        return new BatchedVendorBatchValidator(
            new List<IBatchedVendorBatchRule>
            {
                new BatchedVendorBatchLabelLengthRule(255),
                new BatchedVendorBatchWeightRangeRule(9.0, 4923.0),
                new BatchedVendorBatchBadgeLengthRule(86400),
                new BatchedVendorBatchGradeLengthRule(120),
                new BatchedVendorBatchCodePrefixRule("REF"),
                new BatchedVendorBatchRequiredCodeRule("batched vendor batch needs this field"),
            });
    }
}
