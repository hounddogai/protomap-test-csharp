namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal offboarding tier must pass. Check returns its problems, or none.</summary>
public interface ISeasonalOffboardingTierRule
{
    List<string> Check(SeasonalOffboardingTier item);
}

public sealed record SeasonalOffboardingTierWindowDaysRangeRule(int Min, int Max) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        var valueValue = item.WindowDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"window days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"window days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalOffboardingTierExternalRefLengthRule(int MaxLength) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingTierRequiredSalaryBandRule(string Message) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingTierRequiredCodeRule(string Message) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingTierAllowedExternalRefRule(List<string> Allowed) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalOffboardingTierCodeLengthRule(int MaxLength) : ISeasonalOffboardingTierRule
{
    public List<string> Check(SeasonalOffboardingTier item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal offboarding tier.</summary>
public sealed record SeasonalOffboardingTierValidator(List<ISeasonalOffboardingTierRule> Rules)
{
    public List<string> Validate(SeasonalOffboardingTier item)
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

    public void RequireValid(SeasonalOffboardingTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalOffboardingTier> items)
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

/// <summary>Functions of seasonal offboarding tier rules.</summary>
public static class SeasonalOffboardingTierRules
{
    public static SeasonalOffboardingTierValidator DefaultSeasonalOffboardingTierValidator()
    {
        return new SeasonalOffboardingTierValidator(
            new List<ISeasonalOffboardingTierRule>
            {
                new SeasonalOffboardingTierWindowDaysRangeRule(32, 32),
                new SeasonalOffboardingTierExternalRefLengthRule(3600),
                new SeasonalOffboardingTierRequiredSalaryBandRule("seasonal offboarding tier needs this field"),
                new SeasonalOffboardingTierRequiredCodeRule("seasonal offboarding tier needs this field"),
                new SeasonalOffboardingTierAllowedExternalRefRule(new List<string> { "adaptive", "fallback" }),
                new SeasonalOffboardingTierCodeLengthRule(3600),
            });
    }
}
