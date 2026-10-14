namespace Acme.Workforce.Contractors.Offboarding.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a offboarding request must pass. Check returns its problems, or none.</summary>
public interface IOffboardingRequestRule
{
    List<string> Check(OffboardingRequest item);
}

public sealed record OffboardingRequestCeilingRangeRule(double Min, double Max) : IOffboardingRequestRule
{
    public List<string> Check(OffboardingRequest item)
    {
        var valueValue = item.Ceiling;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ceiling {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRequestCodeLengthRule(int MaxLength) : IOffboardingRequestRule
{
    public List<string> Check(OffboardingRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRequestRequiredTitleRule(string Message) : IOffboardingRequestRule
{
    public List<string> Check(OffboardingRequest item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record OffboardingRequestAllowedSkillRule(List<string> Allowed) : IOffboardingRequestRule
{
    public List<string> Check(OffboardingRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a offboarding request.</summary>
public sealed record OffboardingRequestValidator(List<IOffboardingRequestRule> Rules)
{
    public List<string> Validate(OffboardingRequest item)
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

    public void RequireValid(OffboardingRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<OffboardingRequest> items)
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

/// <summary>Functions of offboarding request rules.</summary>
public static class OffboardingRequestRules
{
    public static OffboardingRequestValidator DefaultOffboardingRequestValidator()
    {
        return new OffboardingRequestValidator(
            new List<IOffboardingRequestRule>
            {
                new OffboardingRequestCeilingRangeRule(7.7, 1171.5),
                new OffboardingRequestCodeLengthRule(80),
                new OffboardingRequestRequiredTitleRule("offboarding request needs a value"),
                new OffboardingRequestAllowedSkillRule(new List<string> { "default", "seasonal" }),
            });
    }
}
