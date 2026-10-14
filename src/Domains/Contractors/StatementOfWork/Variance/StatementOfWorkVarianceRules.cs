namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

/// <summary>A rule a statement of work variance must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkVarianceRule
{
    List<string> Check(StatementOfWorkVariance item);
}

public sealed record StatementOfWorkVarianceTitleLengthRule(int MaxLength) : IStatementOfWorkVarianceRule
{
    public List<string> Check(StatementOfWorkVariance item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkVarianceCodeLengthRule(int MaxLength) : IStatementOfWorkVarianceRule
{
    public List<string> Check(StatementOfWorkVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkVarianceAmountRangeRule(double Min, double Max) : IStatementOfWorkVarianceRule
{
    public List<string> Check(StatementOfWorkVariance item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StatementOfWorkVarianceRequiredTitleRule(string Message) : IStatementOfWorkVarianceRule
{
    public List<string> Check(StatementOfWorkVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkVarianceCountryLengthRule(int MaxLength) : IStatementOfWorkVarianceRule
{
    public List<string> Check(StatementOfWorkVariance item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work variance.</summary>
public sealed record StatementOfWorkVarianceValidator(List<IStatementOfWorkVarianceRule> Rules)
{
    public List<string> Validate(StatementOfWorkVariance item)
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

    public void RequireValid(StatementOfWorkVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkVariance> items)
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

/// <summary>Functions of statement of work variance rules.</summary>
public static class StatementOfWorkVarianceRules
{
    public static StatementOfWorkVarianceValidator DefaultStatementOfWorkVarianceValidator()
    {
        return new StatementOfWorkVarianceValidator(
            new List<IStatementOfWorkVarianceRule>
            {
                new StatementOfWorkVarianceTitleLengthRule(32),
                new StatementOfWorkVarianceCodeLengthRule(80),
                new StatementOfWorkVarianceAmountRangeRule(7.6, 869.3),
                new StatementOfWorkVarianceRequiredTitleRule("statement of work variance needs this field"),
                new StatementOfWorkVarianceCountryLengthRule(16),
            });
    }
}
