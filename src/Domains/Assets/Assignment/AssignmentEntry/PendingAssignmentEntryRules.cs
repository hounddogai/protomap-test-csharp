namespace Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>A rule a pending assignment entry must pass. Check returns its problems, or none.</summary>
public interface IPendingAssignmentEntryRule
{
    List<string> Check(PendingAssignmentEntry item);
}

public sealed record PendingAssignmentEntryMinimumRangeRule(double Min, double Max) : IPendingAssignmentEntryRule
{
    public List<string> Check(PendingAssignmentEntry item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PendingAssignmentEntryRequiredEmployeeIdRule(string Message) : IPendingAssignmentEntryRule
{
    public List<string> Check(PendingAssignmentEntry item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingAssignmentEntryScoreRangeRule(double Min, double Max) : IPendingAssignmentEntryRule
{
    public List<string> Check(PendingAssignmentEntry item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending assignment entry.</summary>
public sealed record PendingAssignmentEntryValidator(List<IPendingAssignmentEntryRule> Rules)
{
    public List<string> Validate(PendingAssignmentEntry item)
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

    public void RequireValid(PendingAssignmentEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingAssignmentEntry> items)
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

/// <summary>Functions of pending assignment entry rules.</summary>
public static class PendingAssignmentEntryRules
{
    public static PendingAssignmentEntryValidator DefaultPendingAssignmentEntryValidator()
    {
        return new PendingAssignmentEntryValidator(
            new List<IPendingAssignmentEntryRule>
            {
                new PendingAssignmentEntryMinimumRangeRule(7.1, 2905.1),
                new PendingAssignmentEntryRequiredEmployeeIdRule("pending assignment entry needs an entry"),
                new PendingAssignmentEntryScoreRangeRule(0.7, 3267.2),
            });
    }
}
