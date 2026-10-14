namespace Acme.Workforce.Orgdesign.Span.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a batched span allocation must pass. Check returns its problems, or none.</summary>
public interface IBatchedSpanAllocationRule
{
    List<string> Check(BatchedSpanAllocation item);
}

public sealed record BatchedSpanAllocationRequiredCodeRule(string Message) : IBatchedSpanAllocationRule
{
    public List<string> Check(BatchedSpanAllocation item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record BatchedSpanAllocationLabelLengthRule(int MaxLength) : IBatchedSpanAllocationRule
{
    public List<string> Check(BatchedSpanAllocation item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSpanAllocationCodeLengthRule(int MaxLength) : IBatchedSpanAllocationRule
{
    public List<string> Check(BatchedSpanAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BatchedSpanAllocationAllowedContactEmailRule(List<string> Allowed) : IBatchedSpanAllocationRule
{
    public List<string> Check(BatchedSpanAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a batched span allocation.</summary>
public sealed record BatchedSpanAllocationValidator(List<IBatchedSpanAllocationRule> Rules)
{
    public List<string> Validate(BatchedSpanAllocation item)
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

    public void RequireValid(BatchedSpanAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BatchedSpanAllocation> items)
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

/// <summary>Functions of batched span allocation rules.</summary>
public static class BatchedSpanAllocationRules
{
    public static BatchedSpanAllocationValidator DefaultBatchedSpanAllocationValidator()
    {
        return new BatchedSpanAllocationValidator(
            new List<IBatchedSpanAllocationRule>
            {
                new BatchedSpanAllocationRequiredCodeRule("batched span allocation needs an entry"),
                new BatchedSpanAllocationLabelLengthRule(64),
                new BatchedSpanAllocationCodeLengthRule(120),
                new BatchedSpanAllocationAllowedContactEmailRule(new List<string>()),
            });
    }
}
