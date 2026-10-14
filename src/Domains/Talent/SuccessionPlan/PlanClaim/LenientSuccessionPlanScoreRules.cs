namespace Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient succession plan score must pass. Check returns its problems, or none.</summary>
public interface ILenientSuccessionPlanScoreRule
{
    List<string> Check(LenientSuccessionPlanScore item);
}

public sealed record LenientSuccessionPlanScoreCodePrefixRule(string Prefix) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanScoreAllowedCountryRule(List<string> Allowed) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Country)))
        {
            return new List<string> { $"country {item.Country} is too long" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanScoreRequiredCodeRule(string Message) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanScoreRevisionRangeRule(int Min, int Max) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanScoreCodeLengthRule(int MaxLength) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientSuccessionPlanScoreRequiredLabelRule(string Message) : ILenientSuccessionPlanScoreRule
{
    public List<string> Check(LenientSuccessionPlanScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient succession plan score.</summary>
public sealed record LenientSuccessionPlanScoreValidator(List<ILenientSuccessionPlanScoreRule> Rules)
{
    public List<string> Validate(LenientSuccessionPlanScore item)
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

    public void RequireValid(LenientSuccessionPlanScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientSuccessionPlanScore> items)
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

/// <summary>Functions of lenient succession plan score rules.</summary>
public static class LenientSuccessionPlanScoreRules
{
    public static LenientSuccessionPlanScoreValidator DefaultLenientSuccessionPlanScoreValidator()
    {
        return new LenientSuccessionPlanScoreValidator(
            new List<ILenientSuccessionPlanScoreRule>
            {
                new LenientSuccessionPlanScoreCodePrefixRule("OPS"),
                new LenientSuccessionPlanScoreAllowedCountryRule(new List<string>()),
                new LenientSuccessionPlanScoreRequiredCodeRule("lenient succession plan score needs this field"),
                new LenientSuccessionPlanScoreRevisionRangeRule(16, 16),
                new LenientSuccessionPlanScoreCodeLengthRule(32),
                new LenientSuccessionPlanScoreRequiredLabelRule("lenient succession plan score needs an entry"),
            });
    }
}
