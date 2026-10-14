namespace Acme.Workforce.Talent.CareerPath.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy career path plan must pass. Check returns its problems, or none.</summary>
public interface ILegacyCareerPathPlanRule
{
    List<string> Check(LegacyCareerPathPlan item);
}

public sealed record LegacyCareerPathPlanCodePrefixRule(string Prefix) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathPlanCodeLengthRule(int MaxLength) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathPlanVersionRangeRule(int Min, int Max) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathPlanWeightRangeRule(double Min, double Max) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        var valueValue = item.Weight;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"weight {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathPlanLabelLengthRule(int MaxLength) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyCareerPathPlanAllowedLabelRule(List<string> Allowed) : ILegacyCareerPathPlanRule
{
    public List<string> Check(LegacyCareerPathPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is stale" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy career path plan.</summary>
public sealed record LegacyCareerPathPlanValidator(List<ILegacyCareerPathPlanRule> Rules)
{
    public List<string> Validate(LegacyCareerPathPlan item)
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

    public void RequireValid(LegacyCareerPathPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyCareerPathPlan> items)
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

/// <summary>Functions of legacy career path plan rules.</summary>
public static class LegacyCareerPathPlanRules
{
    public static LegacyCareerPathPlanValidator DefaultLegacyCareerPathPlanValidator()
    {
        return new LegacyCareerPathPlanValidator(
            new List<ILegacyCareerPathPlanRule>
            {
                new LegacyCareerPathPlanCodePrefixRule("OPS"),
                new LegacyCareerPathPlanCodeLengthRule(3600),
                new LegacyCareerPathPlanVersionRangeRule(1000, 3600),
                new LegacyCareerPathPlanWeightRangeRule(7.1, 4616.4),
                new LegacyCareerPathPlanLabelLengthRule(3600),
                new LegacyCareerPathPlanAllowedLabelRule(new List<string> { "legacy", "tiered", "seasonal" }),
            });
    }
}
