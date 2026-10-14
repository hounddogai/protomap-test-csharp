namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a strict recognition policy must pass. Check returns its problems, or none.</summary>
public interface IStrictRecognitionPolicyRule
{
    List<string> Check(StrictRecognitionPolicy item);
}

public sealed record StrictRecognitionPolicyUpdatedAtRangeRule(int Min, int Max) : IStrictRecognitionPolicyRule
{
    public List<string> Check(StrictRecognitionPolicy item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictRecognitionPolicyAllowedNotesRule(List<string> Allowed) : IStrictRecognitionPolicyRule
{
    public List<string> Check(StrictRecognitionPolicy item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Notes)))
        {
            return new List<string> { $"notes {item.Notes} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record StrictRecognitionPolicyCodePrefixRule(string Prefix) : IStrictRecognitionPolicyRule
{
    public List<string> Check(StrictRecognitionPolicy item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictRecognitionPolicyRequiredContactEmailRule(string Message) : IStrictRecognitionPolicyRule
{
    public List<string> Check(StrictRecognitionPolicy item)
    {
        if (item.ContactEmail.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict recognition policy.</summary>
public sealed record StrictRecognitionPolicyValidator(List<IStrictRecognitionPolicyRule> Rules)
{
    public List<string> Validate(StrictRecognitionPolicy item)
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

    public void RequireValid(StrictRecognitionPolicy item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictRecognitionPolicy> items)
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

/// <summary>Functions of strict recognition policy rules.</summary>
public static class StrictRecognitionPolicyRules
{
    public static StrictRecognitionPolicyValidator DefaultStrictRecognitionPolicyValidator()
    {
        return new StrictRecognitionPolicyValidator(
            new List<IStrictRecognitionPolicyRule>
            {
                new StrictRecognitionPolicyUpdatedAtRangeRule(3600, 86400),
                new StrictRecognitionPolicyAllowedNotesRule(new List<string> { "legacy", "adaptive", "regional" }),
                new StrictRecognitionPolicyCodePrefixRule("Q"),
                new StrictRecognitionPolicyRequiredContactEmailRule("strict recognition policy needs an entry"),
            });
    }
}
