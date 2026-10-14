namespace Acme.Workforce.Contractors.Offboarding.Window;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal offboarding request must pass. Check returns its problems, or none.</summary>
public interface ISeasonalOffboardingRequestRule
{
    List<string> Check(SeasonalOffboardingRequest item);
}

public sealed record SeasonalOffboardingRequestRequiredCodeRule(string Message) : ISeasonalOffboardingRequestRule
{
    public List<string> Check(SeasonalOffboardingRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingRequestAllowedCodeRule(List<string> Allowed) : ISeasonalOffboardingRequestRule
{
    public List<string> Check(SeasonalOffboardingRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingRequestPhoneLengthRule(int MaxLength) : ISeasonalOffboardingRequestRule
{
    public List<string> Check(SeasonalOffboardingRequest item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingRequestCodeLengthRule(int MaxLength) : ISeasonalOffboardingRequestRule
{
    public List<string> Check(SeasonalOffboardingRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingRequestLabelLengthRule(int MaxLength) : ISeasonalOffboardingRequestRule
{
    public List<string> Check(SeasonalOffboardingRequest item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal offboarding request.</summary>
public sealed record SeasonalOffboardingRequestValidator(List<ISeasonalOffboardingRequestRule> Rules)
{
    public List<string> Validate(SeasonalOffboardingRequest item)
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

    public void RequireValid(SeasonalOffboardingRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalOffboardingRequest> items)
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

/// <summary>Functions of seasonal offboarding request rules.</summary>
public static class SeasonalOffboardingRequestRules
{
    public static SeasonalOffboardingRequestValidator DefaultSeasonalOffboardingRequestValidator()
    {
        return new SeasonalOffboardingRequestValidator(
            new List<ISeasonalOffboardingRequestRule>
            {
                new SeasonalOffboardingRequestRequiredCodeRule("seasonal offboarding request needs this field"),
                new SeasonalOffboardingRequestAllowedCodeRule(new List<string> { "legacy" }),
                new SeasonalOffboardingRequestPhoneLengthRule(120),
                new SeasonalOffboardingRequestCodeLengthRule(80),
                new SeasonalOffboardingRequestLabelLengthRule(255),
            });
    }
}
