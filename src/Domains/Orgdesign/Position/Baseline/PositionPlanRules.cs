namespace Acme.Workforce.Orgdesign.Position.Baseline;

/// <summary>A rule a position plan must pass. Check returns its problems, or none.</summary>
public interface IPositionPlanRule
{
    List<string> Check(PositionPlan item);
}

public sealed record PositionPlanRequiredNotesRule(string Message) : IPositionPlanRule
{
    public List<string> Check(PositionPlan item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PositionPlanNotesLengthRule(int MaxLength) : IPositionPlanRule
{
    public List<string> Check(PositionPlan item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPlanCodeLengthRule(int MaxLength) : IPositionPlanRule
{
    public List<string> Check(PositionPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PositionPlanCodePrefixRule(string Prefix) : IPositionPlanRule
{
    public List<string> Check(PositionPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a position plan.</summary>
public sealed record PositionPlanValidator(List<IPositionPlanRule> Rules)
{
    public List<string> Validate(PositionPlan item)
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

    public void RequireValid(PositionPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PositionPlan> items)
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

/// <summary>Functions of position plan rules.</summary>
public static class PositionPlanRules
{
    public static PositionPlanValidator DefaultPositionPlanValidator()
    {
        return new PositionPlanValidator(
            new List<IPositionPlanRule>
            {
                new PositionPlanRequiredNotesRule("position plan needs an entry"),
                new PositionPlanNotesLengthRule(120),
                new PositionPlanCodeLengthRule(32),
                new PositionPlanCodePrefixRule("REF"),
            });
    }
}
