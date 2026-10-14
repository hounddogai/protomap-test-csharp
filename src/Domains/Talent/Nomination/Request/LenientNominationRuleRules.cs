namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a lenient nomination rule must pass. Check returns its problems, or none.</summary>
public interface ILenientNominationRuleRule
{
    List<string> Check(LenientNominationRule item);
}

public sealed record LenientNominationRuleAllowedNameRule(List<string> Allowed) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Name)))
        {
            return new List<string> { $"name {item.Name} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationRuleCeilingRangeRule(double Min, double Max) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        var valueValue = item.Ceiling;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ceiling {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ceiling {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientNominationRuleCurrencyLengthRule(int MaxLength) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationRuleRequiredCodeRule(string Message) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientNominationRuleRatioRangeRule(double Min, double Max) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record LenientNominationRuleCodeLengthRule(int MaxLength) : ILenientNominationRuleRule
{
    public List<string> Check(LenientNominationRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient nomination rule.</summary>
public sealed record LenientNominationRuleValidator(List<ILenientNominationRuleRule> Rules)
{
    public List<string> Validate(LenientNominationRule item)
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

    public void RequireValid(LenientNominationRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientNominationRule> items)
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

/// <summary>Functions of lenient nomination rule rules.</summary>
public static class LenientNominationRuleRules
{
    public static LenientNominationRuleValidator DefaultLenientNominationRuleValidator()
    {
        return new LenientNominationRuleValidator(
            new List<ILenientNominationRuleRule>
            {
                new LenientNominationRuleAllowedNameRule(new List<string> { "primary" }),
                new LenientNominationRuleCeilingRangeRule(6.1, 3115.2),
                new LenientNominationRuleCurrencyLengthRule(3600),
                new LenientNominationRuleRequiredCodeRule("lenient nomination rule needs a value"),
                new LenientNominationRuleRatioRangeRule(0.8, 992.7),
                new LenientNominationRuleCodeLengthRule(1000),
            });
    }
}
