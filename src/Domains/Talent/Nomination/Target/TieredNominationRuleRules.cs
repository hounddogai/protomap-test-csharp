namespace Acme.Workforce.Talent.Nomination.Target;

/// <summary>A rule a tiered nomination rule must pass. Check returns its problems, or none.</summary>
public interface ITieredNominationRuleRule
{
    List<string> Check(TieredNominationRule item);
}

public sealed record TieredNominationRuleSalaryBandLengthRule(int MaxLength) : ITieredNominationRuleRule
{
    public List<string> Check(TieredNominationRule item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationRuleCodePrefixRule(string Prefix) : ITieredNominationRuleRule
{
    public List<string> Check(TieredNominationRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationRuleUnitLengthRule(int MaxLength) : ITieredNominationRuleRule
{
    public List<string> Check(TieredNominationRule item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredNominationRuleRequiredNameRule(string Message) : ITieredNominationRuleRule
{
    public List<string> Check(TieredNominationRule item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered nomination rule.</summary>
public sealed record TieredNominationRuleValidator(List<ITieredNominationRuleRule> Rules)
{
    public List<string> Validate(TieredNominationRule item)
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

    public void RequireValid(TieredNominationRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredNominationRule> items)
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

/// <summary>Functions of tiered nomination rule rules.</summary>
public static class TieredNominationRuleRules
{
    public static TieredNominationRuleValidator DefaultTieredNominationRuleValidator()
    {
        return new TieredNominationRuleValidator(
            new List<ITieredNominationRuleRule>
            {
                new TieredNominationRuleSalaryBandLengthRule(1000),
                new TieredNominationRuleCodePrefixRule("REF"),
                new TieredNominationRuleUnitLengthRule(86400),
                new TieredNominationRuleRequiredNameRule("tiered nomination rule needs an entry"),
            });
    }
}
