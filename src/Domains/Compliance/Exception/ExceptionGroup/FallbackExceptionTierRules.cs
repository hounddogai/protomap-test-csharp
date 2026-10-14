namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a fallback exception tier must pass. Check returns its problems, or none.</summary>
public interface IFallbackExceptionTierRule
{
    List<string> Check(FallbackExceptionTier item);
}

public sealed record FallbackExceptionTierRatingRangeRule(double Min, double Max) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
    {
        var valueValue = item.Rating;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rating {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionTierCodePrefixRule(string Prefix) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionTierRequiredCostCenterRule(string Message) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionTierCostCenterLengthRule(int MaxLength) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackExceptionTierWindowDaysRangeRule(int Min, int Max) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
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

public sealed record FallbackExceptionTierAllowedSkillRule(List<string> Allowed) : IFallbackExceptionTierRule
{
    public List<string> Check(FallbackExceptionTier item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback exception tier.</summary>
public sealed record FallbackExceptionTierValidator(List<IFallbackExceptionTierRule> Rules)
{
    public List<string> Validate(FallbackExceptionTier item)
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

    public void RequireValid(FallbackExceptionTier item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackExceptionTier> items)
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

/// <summary>Functions of fallback exception tier rules.</summary>
public static class FallbackExceptionTierRules
{
    public static FallbackExceptionTierValidator DefaultFallbackExceptionTierValidator()
    {
        return new FallbackExceptionTierValidator(
            new List<IFallbackExceptionTierRule>
            {
                new FallbackExceptionTierRatingRangeRule(8.3, 1590.9),
                new FallbackExceptionTierCodePrefixRule("REF"),
                new FallbackExceptionTierRequiredCostCenterRule("fallback exception tier needs this field"),
                new FallbackExceptionTierCostCenterLengthRule(255),
                new FallbackExceptionTierWindowDaysRangeRule(120, 3600),
                new FallbackExceptionTierAllowedSkillRule(new List<string>()),
            });
    }
}
