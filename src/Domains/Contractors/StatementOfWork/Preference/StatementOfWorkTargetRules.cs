namespace Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>A rule a statement of work target must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkTargetRule
{
    List<string> Check(StatementOfWorkTarget item);
}

public sealed record StatementOfWorkTargetRequiredTitleRule(string Message) : IStatementOfWorkTargetRule
{
    public List<string> Check(StatementOfWorkTarget item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTargetRequiredNameRule(string Message) : IStatementOfWorkTargetRule
{
    public List<string> Check(StatementOfWorkTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkTargetCategoryLengthRule(int MaxLength) : IStatementOfWorkTargetRule
{
    public List<string> Check(StatementOfWorkTarget item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work target.</summary>
public sealed record StatementOfWorkTargetValidator(List<IStatementOfWorkTargetRule> Rules)
{
    public List<string> Validate(StatementOfWorkTarget item)
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

    public void RequireValid(StatementOfWorkTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkTarget> items)
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

/// <summary>Functions of statement of work target rules.</summary>
public static class StatementOfWorkTargetRules
{
    public static StatementOfWorkTargetValidator DefaultStatementOfWorkTargetValidator()
    {
        return new StatementOfWorkTargetValidator(
            new List<IStatementOfWorkTargetRule>
            {
                new StatementOfWorkTargetRequiredTitleRule("statement of work target needs this field"),
                new StatementOfWorkTargetRequiredNameRule("statement of work target needs this field"),
                new StatementOfWorkTargetCategoryLengthRule(255),
            });
    }
}
