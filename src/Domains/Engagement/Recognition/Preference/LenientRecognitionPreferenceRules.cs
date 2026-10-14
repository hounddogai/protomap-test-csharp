namespace Acme.Workforce.Engagement.Recognition.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient recognition preference must pass. Check returns its problems, or none.</summary>
public interface ILenientRecognitionPreferenceRule
{
    List<string> Check(LenientRecognitionPreference item);
}

public sealed record LenientRecognitionPreferenceAllowedSummaryRule(List<string> Allowed) : ILenientRecognitionPreferenceRule
{
    public List<string> Check(LenientRecognitionPreference item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionPreferenceRequiredTitleRule(string Message) : ILenientRecognitionPreferenceRule
{
    public List<string> Check(LenientRecognitionPreference item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientRecognitionPreferenceCodePrefixRule(string Prefix) : ILenientRecognitionPreferenceRule
{
    public List<string> Check(LenientRecognitionPreference item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient recognition preference.</summary>
public sealed record LenientRecognitionPreferenceValidator(List<ILenientRecognitionPreferenceRule> Rules)
{
    public List<string> Validate(LenientRecognitionPreference item)
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

    public void RequireValid(LenientRecognitionPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientRecognitionPreference> items)
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

/// <summary>Functions of lenient recognition preference rules.</summary>
public static class LenientRecognitionPreferenceRules
{
    public static LenientRecognitionPreferenceValidator DefaultLenientRecognitionPreferenceValidator()
    {
        return new LenientRecognitionPreferenceValidator(
            new List<ILenientRecognitionPreferenceRule>
            {
                new LenientRecognitionPreferenceAllowedSummaryRule(new List<string> { "weighted" }),
                new LenientRecognitionPreferenceRequiredTitleRule("lenient recognition preference needs a value"),
                new LenientRecognitionPreferenceCodePrefixRule("ACM"),
            });
    }
}
