namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>A rule a pending statement of work variance must pass. Check returns its problems, or none.</summary>
public interface IPendingStatementOfWorkVarianceRule
{
    List<string> Check(PendingStatementOfWorkVariance item);
}

public sealed record PendingStatementOfWorkVarianceCeilingRangeRule(double Min, double Max) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
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

public sealed record PendingStatementOfWorkVarianceRequiredCodeRule(string Message) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkVarianceRequiredTitleRule(string Message) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkVarianceChannelLengthRule(int MaxLength) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkVarianceCodeLengthRule(int MaxLength) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingStatementOfWorkVarianceManagerIdLengthRule(int MaxLength) : IPendingStatementOfWorkVarianceRule
{
    public List<string> Check(PendingStatementOfWorkVariance item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending statement of work variance.</summary>
public sealed record PendingStatementOfWorkVarianceValidator(List<IPendingStatementOfWorkVarianceRule> Rules)
{
    public List<string> Validate(PendingStatementOfWorkVariance item)
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

    public void RequireValid(PendingStatementOfWorkVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingStatementOfWorkVariance> items)
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

/// <summary>Functions of pending statement of work variance rules.</summary>
public static class PendingStatementOfWorkVarianceRules
{
    public static PendingStatementOfWorkVarianceValidator DefaultPendingStatementOfWorkVarianceValidator()
    {
        return new PendingStatementOfWorkVarianceValidator(
            new List<IPendingStatementOfWorkVarianceRule>
            {
                new PendingStatementOfWorkVarianceCeilingRangeRule(3.7, 343.4),
                new PendingStatementOfWorkVarianceRequiredCodeRule(
                    "pending statement of work variance needs an entry"),
                new PendingStatementOfWorkVarianceRequiredTitleRule(
                    "pending statement of work variance needs this field"),
                new PendingStatementOfWorkVarianceChannelLengthRule(1000),
                new PendingStatementOfWorkVarianceCodeLengthRule(3600),
                new PendingStatementOfWorkVarianceManagerIdLengthRule(80),
            });
    }
}
