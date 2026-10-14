namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback engagement profile must pass. Check returns its problems, or none.</summary>
public interface IFallbackEngagementProfileRule
{
    List<string> Check(FallbackEngagementProfile item);
}

public sealed record FallbackEngagementProfileRequiredGradeRule(string Message) : IFallbackEngagementProfileRule
{
    public List<string> Check(FallbackEngagementProfile item)
    {
        if (item.Grade.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementProfileRequiredNameRule(string Message) : IFallbackEngagementProfileRule
{
    public List<string> Check(FallbackEngagementProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementProfileAllowedCodeRule(List<string> Allowed) : IFallbackEngagementProfileRule
{
    public List<string> Check(FallbackEngagementProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

public sealed record FallbackEngagementProfileCodeLengthRule(int MaxLength) : IFallbackEngagementProfileRule
{
    public List<string> Check(FallbackEngagementProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback engagement profile.</summary>
public sealed record FallbackEngagementProfileValidator(List<IFallbackEngagementProfileRule> Rules)
{
    public List<string> Validate(FallbackEngagementProfile item)
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

    public void RequireValid(FallbackEngagementProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackEngagementProfile> items)
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

/// <summary>Functions of fallback engagement profile rules.</summary>
public static class FallbackEngagementProfileRules
{
    public static FallbackEngagementProfileValidator DefaultFallbackEngagementProfileValidator()
    {
        return new FallbackEngagementProfileValidator(
            new List<IFallbackEngagementProfileRule>
            {
                new FallbackEngagementProfileRequiredGradeRule("fallback engagement profile needs a value"),
                new FallbackEngagementProfileRequiredNameRule("fallback engagement profile needs this field"),
                new FallbackEngagementProfileAllowedCodeRule(new List<string> { "fallback", "batched", "primary" }),
                new FallbackEngagementProfileCodeLengthRule(120),
            });
    }
}
