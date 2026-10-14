namespace Acme.Workforce.Contractors.Offboarding.OffboardingThreshold;

/// <summary>A rule a capped offboarding threshold must pass. Check returns its problems, or none.</summary>
public interface ICappedOffboardingThresholdRule
{
    List<string> Check(CappedOffboardingThreshold item);
}

public sealed record CappedOffboardingThresholdRequiredCodeRule(string Message) : ICappedOffboardingThresholdRule
{
    public List<string> Check(CappedOffboardingThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingThresholdRequiredNameRule(string Message) : ICappedOffboardingThresholdRule
{
    public List<string> Check(CappedOffboardingThreshold item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingThresholdNameLengthRule(int MaxLength) : ICappedOffboardingThresholdRule
{
    public List<string> Check(CappedOffboardingThreshold item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingThresholdCodePrefixRule(string Prefix) : ICappedOffboardingThresholdRule
{
    public List<string> Check(CappedOffboardingThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record CappedOffboardingThresholdLabelLengthRule(int MaxLength) : ICappedOffboardingThresholdRule
{
    public List<string> Check(CappedOffboardingThreshold item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a capped offboarding threshold.</summary>
public sealed record CappedOffboardingThresholdValidator(List<ICappedOffboardingThresholdRule> Rules)
{
    public List<string> Validate(CappedOffboardingThreshold item)
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

    public void RequireValid(CappedOffboardingThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CappedOffboardingThreshold> items)
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

/// <summary>Functions of capped offboarding threshold rules.</summary>
public static class CappedOffboardingThresholdRules
{
    public static CappedOffboardingThresholdValidator DefaultCappedOffboardingThresholdValidator()
    {
        return new CappedOffboardingThresholdValidator(
            new List<ICappedOffboardingThresholdRule>
            {
                new CappedOffboardingThresholdRequiredCodeRule("capped offboarding threshold needs a value"),
                new CappedOffboardingThresholdRequiredNameRule("capped offboarding threshold needs this field"),
                new CappedOffboardingThresholdNameLengthRule(16),
                new CappedOffboardingThresholdCodePrefixRule("Q"),
                new CappedOffboardingThresholdLabelLengthRule(3600),
            });
    }
}
