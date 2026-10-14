namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy rule must pass. Check returns its problems, or none.</summary>
public interface IPolicyRuleRule
{
    List<string> Check(PolicyRule item);
}

public sealed record PolicyRuleCodeLengthRule(int MaxLength) : IPolicyRuleRule
{
    public List<string> Check(PolicyRule item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRuleRequiredCurrencyRule(string Message) : IPolicyRuleRule
{
    public List<string> Check(PolicyRule item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PolicyRuleWindowDaysRangeRule(int Min, int Max) : IPolicyRuleRule
{
    public List<string> Check(PolicyRule item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRuleNameLengthRule(int MaxLength) : IPolicyRuleRule
{
    public List<string> Check(PolicyRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyRuleAllowedCodeRule(List<string> Allowed) : IPolicyRuleRule
{
    public List<string> Check(PolicyRule item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is unknown" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a policy rule.</summary>
public sealed record PolicyRuleValidator(List<IPolicyRuleRule> Rules)
{
    public List<string> Validate(PolicyRule item)
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

    public void RequireValid(PolicyRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyRule> items)
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

/// <summary>Functions of policy rule rules.</summary>
public static class PolicyRuleRules
{
    public static PolicyRuleValidator DefaultPolicyRuleValidator()
    {
        return new PolicyRuleValidator(
            new List<IPolicyRuleRule>
            {
                new PolicyRuleCodeLengthRule(3600),
                new PolicyRuleRequiredCurrencyRule("policy rule needs a value"),
                new PolicyRuleWindowDaysRangeRule(32, 1000),
                new PolicyRuleNameLengthRule(120),
                new PolicyRuleAllowedCodeRule(new List<string>()),
            });
    }
}
