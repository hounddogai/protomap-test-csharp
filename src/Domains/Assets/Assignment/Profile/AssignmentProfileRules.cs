namespace Acme.Workforce.Assets.Assignment.Profile;

/// <summary>A rule a assignment profile must pass. Check returns its problems, or none.</summary>
public interface IAssignmentProfileRule
{
    List<string> Check(AssignmentProfile item);
}

public sealed record AssignmentProfileUnitLengthRule(int MaxLength) : IAssignmentProfileRule
{
    public List<string> Check(AssignmentProfile item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProfileMinimumRangeRule(double Min, double Max) : IAssignmentProfileRule
{
    public List<string> Check(AssignmentProfile item)
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

public sealed record AssignmentProfileScoreRangeRule(double Min, double Max) : IAssignmentProfileRule
{
    public List<string> Check(AssignmentProfile item)
    {
        var valueValue = item.Score;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"score {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AssignmentProfileRequiredUnitRule(string Message) : IAssignmentProfileRule
{
    public List<string> Check(AssignmentProfile item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a assignment profile.</summary>
public sealed record AssignmentProfileValidator(List<IAssignmentProfileRule> Rules)
{
    public List<string> Validate(AssignmentProfile item)
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

    public void RequireValid(AssignmentProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AssignmentProfile> items)
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

/// <summary>Functions of assignment profile rules.</summary>
public static class AssignmentProfileRules
{
    public static AssignmentProfileValidator DefaultAssignmentProfileValidator()
    {
        return new AssignmentProfileValidator(
            new List<IAssignmentProfileRule>
            {
                new AssignmentProfileUnitLengthRule(32),
                new AssignmentProfileMinimumRangeRule(9.8, 1734.0),
                new AssignmentProfileScoreRangeRule(3.0, 1597.9),
                new AssignmentProfileRequiredUnitRule("assignment profile needs an entry"),
            });
    }
}
