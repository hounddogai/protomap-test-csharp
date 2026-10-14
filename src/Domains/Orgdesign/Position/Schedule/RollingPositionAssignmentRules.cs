namespace Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>A rule a rolling position assignment must pass. Check returns its problems, or none.</summary>
public interface IRollingPositionAssignmentRule
{
    List<string> Check(RollingPositionAssignment item);
}

public sealed record RollingPositionAssignmentCapacityRangeRule(int Min, int Max) : IRollingPositionAssignmentRule
{
    public List<string> Check(RollingPositionAssignment item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionAssignmentCodePrefixRule(string Prefix) : IRollingPositionAssignmentRule
{
    public List<string> Check(RollingPositionAssignment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionAssignmentCodeLengthRule(int MaxLength) : IRollingPositionAssignmentRule
{
    public List<string> Check(RollingPositionAssignment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionAssignmentRequiredLabelRule(string Message) : IRollingPositionAssignmentRule
{
    public List<string> Check(RollingPositionAssignment item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionAssignmentLabelLengthRule(int MaxLength) : IRollingPositionAssignmentRule
{
    public List<string> Check(RollingPositionAssignment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling position assignment.</summary>
public sealed record RollingPositionAssignmentValidator(List<IRollingPositionAssignmentRule> Rules)
{
    public List<string> Validate(RollingPositionAssignment item)
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

    public void RequireValid(RollingPositionAssignment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPositionAssignment> items)
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

/// <summary>Functions of rolling position assignment rules.</summary>
public static class RollingPositionAssignmentRules
{
    public static RollingPositionAssignmentValidator DefaultRollingPositionAssignmentValidator()
    {
        return new RollingPositionAssignmentValidator(
            new List<IRollingPositionAssignmentRule>
            {
                new RollingPositionAssignmentCapacityRangeRule(16, 80),
                new RollingPositionAssignmentCodePrefixRule("Q"),
                new RollingPositionAssignmentCodeLengthRule(255),
                new RollingPositionAssignmentRequiredLabelRule("rolling position assignment needs a value"),
                new RollingPositionAssignmentLabelLengthRule(80),
            });
    }
}
