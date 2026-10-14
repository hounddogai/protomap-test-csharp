namespace Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>A rule a strict statement of work assignment must pass. Check returns its problems, or none.</summary>
public interface IStrictStatementOfWorkAssignmentRule
{
    List<string> Check(StrictStatementOfWorkAssignment item);
}

public sealed record StrictStatementOfWorkAssignmentRequiredNameRule(string Message) : IStrictStatementOfWorkAssignmentRule
{
    public List<string> Check(StrictStatementOfWorkAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictStatementOfWorkAssignmentCodePrefixRule(string Prefix) : IStrictStatementOfWorkAssignmentRule
{
    public List<string> Check(StrictStatementOfWorkAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictStatementOfWorkAssignmentStartsAtRangeRule(int Min, int Max) : IStrictStatementOfWorkAssignmentRule
{
    public List<string> Check(StrictStatementOfWorkAssignment item)
    {
        var valueValue = item.StartsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"starts at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"starts at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictStatementOfWorkAssignmentCodeLengthRule(int MaxLength) : IStrictStatementOfWorkAssignmentRule
{
    public List<string> Check(StrictStatementOfWorkAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict statement of work assignment.</summary>
public sealed record StrictStatementOfWorkAssignmentValidator(List<IStrictStatementOfWorkAssignmentRule> Rules)
{
    public List<string> Validate(StrictStatementOfWorkAssignment item)
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

    public void RequireValid(StrictStatementOfWorkAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictStatementOfWorkAssignment> items)
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

/// <summary>Functions of strict statement of work assignment rules.</summary>
public static class StrictStatementOfWorkAssignmentRules
{
    public static StrictStatementOfWorkAssignmentValidator DefaultStrictStatementOfWorkAssignmentValidator()
    {
        return new StrictStatementOfWorkAssignmentValidator(
            new List<IStrictStatementOfWorkAssignmentRule>
            {
                new StrictStatementOfWorkAssignmentRequiredNameRule(
                    "strict statement of work assignment needs a value"),
                new StrictStatementOfWorkAssignmentCodePrefixRule("REF"),
                new StrictStatementOfWorkAssignmentStartsAtRangeRule(32, 80),
                new StrictStatementOfWorkAssignmentCodeLengthRule(64),
            });
    }
}
