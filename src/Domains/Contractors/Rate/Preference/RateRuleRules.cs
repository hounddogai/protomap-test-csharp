namespace Acme.Workforce.Contractors.Rate.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rate rule must pass. Check returns its problems, or none.</summary>
public interface IRateRuleRule
{
    List<string> Check(RateRule item);
}

public sealed record RateRuleNameLengthRule(int MaxLength) : IRateRuleRule
{
    public List<string> Check(RateRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateRuleAllowedLocaleRule(List<string> Allowed) : IRateRuleRule
{
    public List<string> Check(RateRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Locale)))
        {
            return new List<string> { $"locale {item.Locale} is expired" };
        }
        return new List<string>();
    }
}

public sealed record RateRuleLocaleLengthRule(int MaxLength) : IRateRuleRule
{
    public List<string> Check(RateRule item)
    {
        var size = item.Locale.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"locale is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rate rule.</summary>
public sealed record RateRuleValidator(List<IRateRuleRule> Rules)
{
    public List<string> Validate(RateRule item)
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

    public void RequireValid(RateRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateRule> items)
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

/// <summary>Functions of rate rule rules.</summary>
public static class RateRuleRules
{
    public static RateRuleValidator DefaultRateRuleValidator()
    {
        return new RateRuleValidator(
            new List<IRateRuleRule>
            {
                new RateRuleNameLengthRule(16),
                new RateRuleAllowedLocaleRule(new List<string> { "lenient" }),
                new RateRuleLocaleLengthRule(16),
            });
    }
}
