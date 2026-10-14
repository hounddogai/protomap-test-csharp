namespace Acme.Workforce.Orgdesign.Position.Policy;

/// <summary>A rule a position target must pass. Check returns its problems, or none.</summary>
public interface IPositionTargetRule
{
    List<string> Check(PositionTarget item);
}

public sealed record PositionTargetRequiredNameRule(string Message) : IPositionTargetRule
{
    public List<string> Check(PositionTarget item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionTargetRequiredDepartmentRule(string Message) : IPositionTargetRule
{
    public List<string> Check(PositionTarget item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionTargetNameLengthRule(int MaxLength) : IPositionTargetRule
{
    public List<string> Check(PositionTarget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position target.</summary>
public sealed record PositionTargetValidator(List<IPositionTargetRule> Rules)
{
    public List<string> Validate(PositionTarget item)
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

    public void RequireValid(PositionTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionTarget> items)
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

/// <summary>Functions of position target rules.</summary>
public static class PositionTargetRules
{
    public static PositionTargetValidator DefaultPositionTargetValidator()
    {
        return new PositionTargetValidator(
            new List<IPositionTargetRule>
            {
                new PositionTargetRequiredNameRule("position target needs a value"),
                new PositionTargetRequiredDepartmentRule("position target needs an entry"),
                new PositionTargetNameLengthRule(255),
            });
    }
}
