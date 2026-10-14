namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a legacy garnishment metric must pass. Check returns its problems, or none.</summary>
public interface ILegacyGarnishmentMetricRule
{
    List<string> Check(LegacyGarnishmentMetric item);
}

public sealed record LegacyGarnishmentMetricRequiredSkillRule(string Message) : ILegacyGarnishmentMetricRule
{
    public List<string> Check(LegacyGarnishmentMetric item)
    {
        if (item.Skill.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentMetricCodePrefixRule(string Prefix) : ILegacyGarnishmentMetricRule
{
    public List<string> Check(LegacyGarnishmentMetric item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentMetricAllowedSkillRule(List<string> Allowed) : ILegacyGarnishmentMetricRule
{
    public List<string> Check(LegacyGarnishmentMetric item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Skill)))
        {
            return new List<string> { $"skill {item.Skill} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record LegacyGarnishmentMetricLabelLengthRule(int MaxLength) : ILegacyGarnishmentMetricRule
{
    public List<string> Check(LegacyGarnishmentMetric item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a legacy garnishment metric.</summary>
public sealed record LegacyGarnishmentMetricValidator(List<ILegacyGarnishmentMetricRule> Rules)
{
    public List<string> Validate(LegacyGarnishmentMetric item)
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

    public void RequireValid(LegacyGarnishmentMetric item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyGarnishmentMetric> items)
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

/// <summary>Functions of legacy garnishment metric rules.</summary>
public static class LegacyGarnishmentMetricRules
{
    public static LegacyGarnishmentMetricValidator DefaultLegacyGarnishmentMetricValidator()
    {
        return new LegacyGarnishmentMetricValidator(
            new List<ILegacyGarnishmentMetricRule>
            {
                new LegacyGarnishmentMetricRequiredSkillRule("legacy garnishment metric needs an entry"),
                new LegacyGarnishmentMetricCodePrefixRule("REF"),
                new LegacyGarnishmentMetricAllowedSkillRule(new List<string> { "global", "fallback" }),
                new LegacyGarnishmentMetricLabelLengthRule(3600),
            });
    }
}
