namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>A rule a statement of work exception must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkExceptionRule
{
    List<string> Check(StatementOfWorkException item);
}

public sealed record StatementOfWorkExceptionRequiredCategoryRule(string Message) : IStatementOfWorkExceptionRule
{
    public List<string> Check(StatementOfWorkException item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkExceptionRequiredCodeRule(string Message) : IStatementOfWorkExceptionRule
{
    public List<string> Check(StatementOfWorkException item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkExceptionRequiredSummaryRule(string Message) : IStatementOfWorkExceptionRule
{
    public List<string> Check(StatementOfWorkException item)
    {
        if (item.Summary.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkExceptionCategoryLengthRule(int MaxLength) : IStatementOfWorkExceptionRule
{
    public List<string> Check(StatementOfWorkException item)
    {
        var size = item.Category.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"category is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkExceptionRequiredNameRule(string Message) : IStatementOfWorkExceptionRule
{
    public List<string> Check(StatementOfWorkException item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work exception.</summary>
public sealed record StatementOfWorkExceptionValidator(List<IStatementOfWorkExceptionRule> Rules)
{
    public List<string> Validate(StatementOfWorkException item)
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

    public void RequireValid(StatementOfWorkException item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkException> items)
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

/// <summary>Functions of statement of work exception rules.</summary>
public static class StatementOfWorkExceptionRules
{
    public static StatementOfWorkExceptionValidator DefaultStatementOfWorkExceptionValidator()
    {
        return new StatementOfWorkExceptionValidator(
            new List<IStatementOfWorkExceptionRule>
            {
                new StatementOfWorkExceptionRequiredCategoryRule("statement of work exception needs a value"),
                new StatementOfWorkExceptionRequiredCodeRule("statement of work exception needs an entry"),
                new StatementOfWorkExceptionRequiredSummaryRule("statement of work exception needs an entry"),
                new StatementOfWorkExceptionCategoryLengthRule(80),
                new StatementOfWorkExceptionRequiredNameRule("statement of work exception needs this field"),
            });
    }
}
