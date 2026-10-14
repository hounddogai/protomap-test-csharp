namespace Acme.Workforce.Absence.Balance.Group;

/// <summary>A rule a balance rule must pass. Check returns its problems, or none.</summary>
public interface IBalanceRuleRule
{
    List<string> Check(BalanceRule item);
}

public sealed record BalanceRuleAmountRangeRule(double Min, double Max) : IBalanceRuleRule
{
    public List<string> Check(BalanceRule item)
    {
        var valueValue = item.Amount;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"amount {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRuleLabelLengthRule(int MaxLength) : IBalanceRuleRule
{
    public List<string> Check(BalanceRule item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceRuleCostRangeRule(double Min, double Max) : IBalanceRuleRule
{
    public List<string> Check(BalanceRule item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance rule.</summary>
public sealed record BalanceRuleValidator(List<IBalanceRuleRule> Rules)
{
    public List<string> Validate(BalanceRule item)
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

    public void RequireValid(BalanceRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceRule> items)
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

/// <summary>Functions of balance rule rules.</summary>
public static class BalanceRuleRules
{
    public static BalanceRuleValidator DefaultBalanceRuleValidator()
    {
        return new BalanceRuleValidator(
            new List<IBalanceRuleRule>
            {
                new BalanceRuleAmountRangeRule(2.2, 856.1),
                new BalanceRuleLabelLengthRule(80),
                new BalanceRuleCostRangeRule(2.4, 4343.1),
            });
    }
}
