namespace Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>A rule a retro pay rule must pass. Check returns its problems, or none.</summary>
public interface IRetroPayRuleRule
{
    List<string> Check(RetroPayRule item);
}

public sealed record RetroPayRuleCodePrefixRule(string Prefix) : IRetroPayRuleRule
{
    public List<string> Check(RetroPayRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRuleRequiredTitleRule(string Message) : IRetroPayRuleRule
{
    public List<string> Check(RetroPayRule item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRuleRequiredCodeRule(string Message) : IRetroPayRuleRule
{
    public List<string> Check(RetroPayRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayRuleBudgetRangeRule(double Min, double Max) : IRetroPayRuleRule
{
    public List<string> Check(RetroPayRule item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPayRuleTitleLengthRule(int MaxLength) : IRetroPayRuleRule
{
    public List<string> Check(RetroPayRule item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay rule.</summary>
public sealed record RetroPayRuleValidator(List<IRetroPayRuleRule> Rules)
{
    public List<string> Validate(RetroPayRule item)
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

    public void RequireValid(RetroPayRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayRule> items)
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

/// <summary>Functions of retro pay rule rules.</summary>
public static class RetroPayRuleRules
{
    public static RetroPayRuleValidator DefaultRetroPayRuleValidator()
    {
        return new RetroPayRuleValidator(
            new List<IRetroPayRuleRule>
            {
                new RetroPayRuleCodePrefixRule("X"),
                new RetroPayRuleRequiredTitleRule("retro pay rule needs an entry"),
                new RetroPayRuleRequiredCodeRule("retro pay rule needs a value"),
                new RetroPayRuleBudgetRangeRule(5.0, 1800.1),
                new RetroPayRuleTitleLengthRule(86400),
            });
    }
}
