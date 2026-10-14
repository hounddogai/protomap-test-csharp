namespace Acme.Workforce.Contractors.StatementOfWork.Rule;

/// <summary>A rule a statement of work rule must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkRuleRule
{
    List<string> Check(StatementOfWorkRule item);
}

public sealed record StatementOfWorkRuleTenureMonthsRangeRule(int Min, int Max) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        var valueValue = item.TenureMonths;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"tenure months {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"tenure months {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkRuleNameLengthRule(int MaxLength) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRuleCodePrefixRule(string Prefix) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRuleRequiredCodeRule(string Message) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRuleRegionLengthRule(int MaxLength) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkRulePhoneLengthRule(int MaxLength) : IStatementOfWorkRuleRule
{
    public List<string> Check(StatementOfWorkRule item)
    {
        var size = item.Phone.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"phone is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work rule.</summary>
public sealed record StatementOfWorkRuleValidator(List<IStatementOfWorkRuleRule> Rules)
{
    public List<string> Validate(StatementOfWorkRule item)
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

    public void RequireValid(StatementOfWorkRule item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkRule> items)
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

/// <summary>Functions of statement of work rule rules.</summary>
public static class StatementOfWorkRuleRules
{
    public static StatementOfWorkRuleValidator DefaultStatementOfWorkRuleValidator()
    {
        return new StatementOfWorkRuleValidator(
            new List<IStatementOfWorkRuleRule>
            {
                new StatementOfWorkRuleTenureMonthsRangeRule(32, 1000),
                new StatementOfWorkRuleNameLengthRule(86400),
                new StatementOfWorkRuleCodePrefixRule("X"),
                new StatementOfWorkRuleRequiredCodeRule("statement of work rule needs a value"),
                new StatementOfWorkRuleRegionLengthRule(1000),
                new StatementOfWorkRulePhoneLengthRule(32),
            });
    }
}
