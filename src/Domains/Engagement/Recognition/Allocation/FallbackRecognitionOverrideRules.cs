namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback recognition override must pass. Check returns its problems, or none.</summary>
public interface IFallbackRecognitionOverrideRule
{
    List<string> Check(FallbackRecognitionOverride item);
}

public sealed record FallbackRecognitionOverrideCodePrefixRule(string Prefix) : IFallbackRecognitionOverrideRule
{
    public List<string> Check(FallbackRecognitionOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionOverrideRequiredRegionRule(string Message) : IFallbackRecognitionOverrideRule
{
    public List<string> Check(FallbackRecognitionOverride item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionOverrideAllowedCodeRule(List<string> Allowed) : IFallbackRecognitionOverrideRule
{
    public List<string> Check(FallbackRecognitionOverride item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionOverrideCodeLengthRule(int MaxLength) : IFallbackRecognitionOverrideRule
{
    public List<string> Check(FallbackRecognitionOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackRecognitionOverrideRequiredEmployeeIdRule(string Message) : IFallbackRecognitionOverrideRule
{
    public List<string> Check(FallbackRecognitionOverride item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback recognition override.</summary>
public sealed record FallbackRecognitionOverrideValidator(List<IFallbackRecognitionOverrideRule> Rules)
{
    public List<string> Validate(FallbackRecognitionOverride item)
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

    public void RequireValid(FallbackRecognitionOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackRecognitionOverride> items)
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

/// <summary>Functions of fallback recognition override rules.</summary>
public static class FallbackRecognitionOverrideRules
{
    public static FallbackRecognitionOverrideValidator DefaultFallbackRecognitionOverrideValidator()
    {
        return new FallbackRecognitionOverrideValidator(
            new List<IFallbackRecognitionOverrideRule>
            {
                new FallbackRecognitionOverrideCodePrefixRule("X"),
                new FallbackRecognitionOverrideRequiredRegionRule("fallback recognition override needs this field"),
                new FallbackRecognitionOverrideAllowedCodeRule(new List<string> { "regional" }),
                new FallbackRecognitionOverrideCodeLengthRule(255),
                new FallbackRecognitionOverrideRequiredEmployeeIdRule("fallback recognition override needs an entry"),
            });
    }
}
