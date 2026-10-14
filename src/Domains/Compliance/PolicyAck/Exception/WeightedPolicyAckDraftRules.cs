namespace Acme.Workforce.Compliance.PolicyAck.Exception;

/// <summary>A rule a weighted policy ack draft must pass. Check returns its problems, or none.</summary>
public interface IWeightedPolicyAckDraftRule
{
    List<string> Check(WeightedPolicyAckDraft item);
}

public sealed record WeightedPolicyAckDraftCodePrefixRule(string Prefix) : IWeightedPolicyAckDraftRule
{
    public List<string> Check(WeightedPolicyAckDraft item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckDraftCategoryLengthRule(int MaxLength) : IWeightedPolicyAckDraftRule
{
    public List<string> Check(WeightedPolicyAckDraft item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckDraftTitleLengthRule(int MaxLength) : IWeightedPolicyAckDraftRule
{
    public List<string> Check(WeightedPolicyAckDraft item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedPolicyAckDraftRequiredOwnerIdRule(string Message) : IWeightedPolicyAckDraftRule
{
    public List<string> Check(WeightedPolicyAckDraft item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted policy ack draft.</summary>
public sealed record WeightedPolicyAckDraftValidator(List<IWeightedPolicyAckDraftRule> Rules)
{
    public List<string> Validate(WeightedPolicyAckDraft item)
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

    public void RequireValid(WeightedPolicyAckDraft item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedPolicyAckDraft> items)
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

/// <summary>Functions of weighted policy ack draft rules.</summary>
public static class WeightedPolicyAckDraftRules
{
    public static WeightedPolicyAckDraftValidator DefaultWeightedPolicyAckDraftValidator()
    {
        return new WeightedPolicyAckDraftValidator(
            new List<IWeightedPolicyAckDraftRule>
            {
                new WeightedPolicyAckDraftCodePrefixRule("ACM"),
                new WeightedPolicyAckDraftCategoryLengthRule(16),
                new WeightedPolicyAckDraftTitleLengthRule(64),
                new WeightedPolicyAckDraftRequiredOwnerIdRule("weighted policy ack draft needs an entry"),
            });
    }
}
