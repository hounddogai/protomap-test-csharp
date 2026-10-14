namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>A rule a manual statement of work profile must pass. Check returns its problems, or none.</summary>
public interface IManualStatementOfWorkProfileRule
{
    List<string> Check(ManualStatementOfWorkProfile item);
}

public sealed record ManualStatementOfWorkProfileTitleLengthRule(int MaxLength) : IManualStatementOfWorkProfileRule
{
    public List<string> Check(ManualStatementOfWorkProfile item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualStatementOfWorkProfileRequiredTitleRule(string Message) : IManualStatementOfWorkProfileRule
{
    public List<string> Check(ManualStatementOfWorkProfile item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualStatementOfWorkProfilePriorityRangeRule(int Min, int Max) : IManualStatementOfWorkProfileRule
{
    public List<string> Check(ManualStatementOfWorkProfile item)
    {
        var valueValue = item.Priority;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"priority {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"priority {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualStatementOfWorkProfileJobCodeLengthRule(int MaxLength) : IManualStatementOfWorkProfileRule
{
    public List<string> Check(ManualStatementOfWorkProfile item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual statement of work profile.</summary>
public sealed record ManualStatementOfWorkProfileValidator(List<IManualStatementOfWorkProfileRule> Rules)
{
    public List<string> Validate(ManualStatementOfWorkProfile item)
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

    public void RequireValid(ManualStatementOfWorkProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualStatementOfWorkProfile> items)
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

/// <summary>Functions of manual statement of work profile rules.</summary>
public static class ManualStatementOfWorkProfileRules
{
    public static ManualStatementOfWorkProfileValidator DefaultManualStatementOfWorkProfileValidator()
    {
        return new ManualStatementOfWorkProfileValidator(
            new List<IManualStatementOfWorkProfileRule>
            {
                new ManualStatementOfWorkProfileTitleLengthRule(255),
                new ManualStatementOfWorkProfileRequiredTitleRule("manual statement of work profile needs a value"),
                new ManualStatementOfWorkProfilePriorityRangeRule(120, 120),
                new ManualStatementOfWorkProfileJobCodeLengthRule(3600),
            });
    }
}
