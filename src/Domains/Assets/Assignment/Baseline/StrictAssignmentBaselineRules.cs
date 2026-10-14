namespace Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>A rule a strict assignment baseline must pass. Check returns its problems, or none.</summary>
public interface IStrictAssignmentBaselineRule
{
    List<string> Check(StrictAssignmentBaseline item);
}

public sealed record StrictAssignmentBaselineMaximumRangeRule(double Min, double Max) : IStrictAssignmentBaselineRule
{
    public List<string> Check(StrictAssignmentBaseline item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictAssignmentBaselineNameLengthRule(int MaxLength) : IStrictAssignmentBaselineRule
{
    public List<string> Check(StrictAssignmentBaseline item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentBaselineBudgetRangeRule(double Min, double Max) : IStrictAssignmentBaselineRule
{
    public List<string> Check(StrictAssignmentBaseline item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentBaselineCodeLengthRule(int MaxLength) : IStrictAssignmentBaselineRule
{
    public List<string> Check(StrictAssignmentBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictAssignmentBaselineSkillLengthRule(int MaxLength) : IStrictAssignmentBaselineRule
{
    public List<string> Check(StrictAssignmentBaseline item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a strict assignment baseline.</summary>
public sealed record StrictAssignmentBaselineValidator(List<IStrictAssignmentBaselineRule> Rules)
{
    public List<string> Validate(StrictAssignmentBaseline item)
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

    public void RequireValid(StrictAssignmentBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictAssignmentBaseline> items)
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

/// <summary>Functions of strict assignment baseline rules.</summary>
public static class StrictAssignmentBaselineRules
{
    public static StrictAssignmentBaselineValidator DefaultStrictAssignmentBaselineValidator()
    {
        return new StrictAssignmentBaselineValidator(
            new List<IStrictAssignmentBaselineRule>
            {
                new StrictAssignmentBaselineMaximumRangeRule(2.3, 1108.6),
                new StrictAssignmentBaselineNameLengthRule(255),
                new StrictAssignmentBaselineBudgetRangeRule(2.9, 3213.7),
                new StrictAssignmentBaselineCodeLengthRule(86400),
                new StrictAssignmentBaselineSkillLengthRule(255),
            });
    }
}
