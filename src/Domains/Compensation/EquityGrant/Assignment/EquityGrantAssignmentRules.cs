namespace Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>A rule a equity grant assignment must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantAssignmentRule
{
    List<string> Check(EquityGrantAssignment item);
}

public sealed record EquityGrantAssignmentRequiredNameRule(string Message) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAssignmentScoreRangeRule(double Min, double Max) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantAssignmentRequiredCodeRule(string Message) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAssignmentNameLengthRule(int MaxLength) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAssignmentManagerIdLengthRule(int MaxLength) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        var size = item.ManagerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"manager id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantAssignmentHoursRangeRule(double Min, double Max) : IEquityGrantAssignmentRule
{
    public List<string> Check(EquityGrantAssignment item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a equity grant assignment.</summary>
public sealed record EquityGrantAssignmentValidator(List<IEquityGrantAssignmentRule> Rules)
{
    public List<string> Validate(EquityGrantAssignment item)
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

    public void RequireValid(EquityGrantAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantAssignment> items)
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

/// <summary>Functions of equity grant assignment rules.</summary>
public static class EquityGrantAssignmentRules
{
    public static EquityGrantAssignmentValidator DefaultEquityGrantAssignmentValidator()
    {
        return new EquityGrantAssignmentValidator(
            new List<IEquityGrantAssignmentRule>
            {
                new EquityGrantAssignmentRequiredNameRule("equity grant assignment needs this field"),
                new EquityGrantAssignmentScoreRangeRule(0.3, 2051.9),
                new EquityGrantAssignmentRequiredCodeRule("equity grant assignment needs an entry"),
                new EquityGrantAssignmentNameLengthRule(80),
                new EquityGrantAssignmentManagerIdLengthRule(255),
                new EquityGrantAssignmentHoursRangeRule(4.7, 1130.7),
            });
    }
}
