namespace Acme.Workforce.Absence.Balance.Assignment;

/// <summary>A rule a balance assignment must pass. Check returns its problems, or none.</summary>
public interface IBalanceAssignmentRule
{
    List<string> Check(BalanceAssignment item);
}

public sealed record BalanceAssignmentCodeLengthRule(int MaxLength) : IBalanceAssignmentRule
{
    public List<string> Check(BalanceAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAssignmentTitleLengthRule(int MaxLength) : IBalanceAssignmentRule
{
    public List<string> Check(BalanceAssignment item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAssignmentSourceLengthRule(int MaxLength) : IBalanceAssignmentRule
{
    public List<string> Check(BalanceAssignment item)
    {
        var size = item.Source.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"source is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record BalanceAssignmentMinimumRangeRule(double Min, double Max) : IBalanceAssignmentRule
{
    public List<string> Check(BalanceAssignment item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record BalanceAssignmentRequiredTitleRule(string Message) : IBalanceAssignmentRule
{
    public List<string> Check(BalanceAssignment item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a balance assignment.</summary>
public sealed record BalanceAssignmentValidator(List<IBalanceAssignmentRule> Rules)
{
    public List<string> Validate(BalanceAssignment item)
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

    public void RequireValid(BalanceAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<BalanceAssignment> items)
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

/// <summary>Functions of balance assignment rules.</summary>
public static class BalanceAssignmentRules
{
    public static BalanceAssignmentValidator DefaultBalanceAssignmentValidator()
    {
        return new BalanceAssignmentValidator(
            new List<IBalanceAssignmentRule>
            {
                new BalanceAssignmentCodeLengthRule(1000),
                new BalanceAssignmentTitleLengthRule(3600),
                new BalanceAssignmentSourceLengthRule(255),
                new BalanceAssignmentMinimumRangeRule(9.2, 3019.5),
                new BalanceAssignmentRequiredTitleRule("balance assignment needs a value"),
            });
    }
}
