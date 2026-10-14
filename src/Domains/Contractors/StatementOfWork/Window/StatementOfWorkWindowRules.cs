namespace Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>A rule a statement of work window must pass. Check returns its problems, or none.</summary>
public interface IStatementOfWorkWindowRule
{
    List<string> Check(StatementOfWorkWindow item);
}

public sealed record StatementOfWorkWindowRequiredManagerIdRule(string Message) : IStatementOfWorkWindowRule
{
    public List<string> Check(StatementOfWorkWindow item)
    {
        if (item.ManagerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkWindowManagerIdLengthRule(int MaxLength) : IStatementOfWorkWindowRule
{
    public List<string> Check(StatementOfWorkWindow item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkWindowChannelLengthRule(int MaxLength) : IStatementOfWorkWindowRule
{
    public List<string> Check(StatementOfWorkWindow item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StatementOfWorkWindowRequiredCodeRule(string Message) : IStatementOfWorkWindowRule
{
    public List<string> Check(StatementOfWorkWindow item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a statement of work window.</summary>
public sealed record StatementOfWorkWindowValidator(List<IStatementOfWorkWindowRule> Rules)
{
    public List<string> Validate(StatementOfWorkWindow item)
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

    public void RequireValid(StatementOfWorkWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StatementOfWorkWindow> items)
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

/// <summary>Functions of statement of work window rules.</summary>
public static class StatementOfWorkWindowRules
{
    public static StatementOfWorkWindowValidator DefaultStatementOfWorkWindowValidator()
    {
        return new StatementOfWorkWindowValidator(
            new List<IStatementOfWorkWindowRule>
            {
                new StatementOfWorkWindowRequiredManagerIdRule("statement of work window needs an entry"),
                new StatementOfWorkWindowManagerIdLengthRule(1000),
                new StatementOfWorkWindowChannelLengthRule(16),
                new StatementOfWorkWindowRequiredCodeRule("statement of work window needs a value"),
            });
    }
}
