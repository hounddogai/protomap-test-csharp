namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a scenario segment must pass. Check returns its problems, or none.</summary>
public interface IScenarioSegmentRule
{
    List<string> Check(ScenarioSegment item);
}

public sealed record ScenarioSegmentCodeLengthRule(int MaxLength) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSegmentRequiredPhoneRule(string Message) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSegmentNameLengthRule(int MaxLength) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSegmentFteRangeRule(double Min, double Max) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ScenarioSegmentAllowedSummaryRule(List<string> Allowed) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Summary)))
        {
            return new List<string> { $"summary {item.Summary} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ScenarioSegmentRequiredCodeRule(string Message) : IScenarioSegmentRule
{
    public List<string> Check(ScenarioSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a scenario segment.</summary>
public sealed record ScenarioSegmentValidator(List<IScenarioSegmentRule> Rules)
{
    public List<string> Validate(ScenarioSegment item)
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

    public void RequireValid(ScenarioSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ScenarioSegment> items)
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

/// <summary>Functions of scenario segment rules.</summary>
public static class ScenarioSegmentRules
{
    public static ScenarioSegmentValidator DefaultScenarioSegmentValidator()
    {
        return new ScenarioSegmentValidator(
            new List<IScenarioSegmentRule>
            {
                new ScenarioSegmentCodeLengthRule(32),
                new ScenarioSegmentRequiredPhoneRule("scenario segment needs a value"),
                new ScenarioSegmentNameLengthRule(16),
                new ScenarioSegmentFteRangeRule(3.8, 1295.8),
                new ScenarioSegmentAllowedSummaryRule(new List<string> { "primary", "strict", "primary" }),
                new ScenarioSegmentRequiredCodeRule("scenario segment needs an entry"),
            });
    }
}
