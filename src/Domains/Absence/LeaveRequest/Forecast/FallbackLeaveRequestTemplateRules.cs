namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

/// <summary>A rule a fallback leave request template must pass. Check returns its problems, or none.</summary>
public interface IFallbackLeaveRequestTemplateRule
{
    List<string> Check(FallbackLeaveRequestTemplate item);
}

public sealed record FallbackLeaveRequestTemplateRequiredCodeRule(string Message) : IFallbackLeaveRequestTemplateRule
{
    public List<string> Check(FallbackLeaveRequestTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestTemplateGradeLengthRule(int MaxLength) : IFallbackLeaveRequestTemplateRule
{
    public List<string> Check(FallbackLeaveRequestTemplate item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackLeaveRequestTemplateCodePrefixRule(string Prefix) : IFallbackLeaveRequestTemplateRule
{
    public List<string> Check(FallbackLeaveRequestTemplate item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback leave request template.</summary>
public sealed record FallbackLeaveRequestTemplateValidator(List<IFallbackLeaveRequestTemplateRule> Rules)
{
    public List<string> Validate(FallbackLeaveRequestTemplate item)
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

    public void RequireValid(FallbackLeaveRequestTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackLeaveRequestTemplate> items)
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

/// <summary>Functions of fallback leave request template rules.</summary>
public static class FallbackLeaveRequestTemplateRules
{
    public static FallbackLeaveRequestTemplateValidator DefaultFallbackLeaveRequestTemplateValidator()
    {
        return new FallbackLeaveRequestTemplateValidator(
            new List<IFallbackLeaveRequestTemplateRule>
            {
                new FallbackLeaveRequestTemplateRequiredCodeRule("fallback leave request template needs this field"),
                new FallbackLeaveRequestTemplateGradeLengthRule(255),
                new FallbackLeaveRequestTemplateCodePrefixRule("X"),
            });
    }
}
