namespace Acme.Workforce.Engagement.Feedback.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending feedback mapping must pass. Check returns its problems, or none.</summary>
public interface IPendingFeedbackMappingRule
{
    List<string> Check(PendingFeedbackMapping item);
}

public sealed record PendingFeedbackMappingAllowedContactEmailRule(List<string> Allowed) : IPendingFeedbackMappingRule
{
    public List<string> Check(PendingFeedbackMapping item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ContactEmail)))
        {
            return new List<string> { $"contact email {item.ContactEmail} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingFeedbackMappingRequiredNameRule(string Message) : IPendingFeedbackMappingRule
{
    public List<string> Check(PendingFeedbackMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingFeedbackMappingExpiresAtRangeRule(int Min, int Max) : IPendingFeedbackMappingRule
{
    public List<string> Check(PendingFeedbackMapping item)
    {
        var valueValue = item.ExpiresAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"expires at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingFeedbackMappingRequiredTitleRule(string Message) : IPendingFeedbackMappingRule
{
    public List<string> Check(PendingFeedbackMapping item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingFeedbackMappingRequiredCodeRule(string Message) : IPendingFeedbackMappingRule
{
    public List<string> Check(PendingFeedbackMapping item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending feedback mapping.</summary>
public sealed record PendingFeedbackMappingValidator(List<IPendingFeedbackMappingRule> Rules)
{
    public List<string> Validate(PendingFeedbackMapping item)
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

    public void RequireValid(PendingFeedbackMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingFeedbackMapping> items)
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

/// <summary>Functions of pending feedback mapping rules.</summary>
public static class PendingFeedbackMappingRules
{
    public static PendingFeedbackMappingValidator DefaultPendingFeedbackMappingValidator()
    {
        return new PendingFeedbackMappingValidator(
            new List<IPendingFeedbackMappingRule>
            {
                new PendingFeedbackMappingAllowedContactEmailRule(new List<string> { "seasonal", "regional" }),
                new PendingFeedbackMappingRequiredNameRule("pending feedback mapping needs an entry"),
                new PendingFeedbackMappingExpiresAtRangeRule(86400, 86400),
                new PendingFeedbackMappingRequiredTitleRule("pending feedback mapping needs a value"),
                new PendingFeedbackMappingRequiredCodeRule("pending feedback mapping needs this field"),
            });
    }
}
