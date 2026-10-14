namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

/// <summary>A rule a lenient action plan bundle must pass. Check returns its problems, or none.</summary>
public interface ILenientActionPlanBundleRule
{
    List<string> Check(LenientActionPlanBundle item);
}

public sealed record LenientActionPlanBundleRequiredNameRule(string Message) : ILenientActionPlanBundleRule
{
    public List<string> Check(LenientActionPlanBundle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanBundleNameLengthRule(int MaxLength) : ILenientActionPlanBundleRule
{
    public List<string> Check(LenientActionPlanBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientActionPlanBundleCodePrefixRule(string Prefix) : ILenientActionPlanBundleRule
{
    public List<string> Check(LenientActionPlanBundle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient action plan bundle.</summary>
public sealed record LenientActionPlanBundleValidator(List<ILenientActionPlanBundleRule> Rules)
{
    public List<string> Validate(LenientActionPlanBundle item)
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

    public void RequireValid(LenientActionPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientActionPlanBundle> items)
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

/// <summary>Functions of lenient action plan bundle rules.</summary>
public static class LenientActionPlanBundleRules
{
    public static LenientActionPlanBundleValidator DefaultLenientActionPlanBundleValidator()
    {
        return new LenientActionPlanBundleValidator(
            new List<ILenientActionPlanBundleRule>
            {
                new LenientActionPlanBundleRequiredNameRule("lenient action plan bundle needs this field"),
                new LenientActionPlanBundleNameLengthRule(3600),
                new LenientActionPlanBundleCodePrefixRule("REF"),
            });
    }
}
