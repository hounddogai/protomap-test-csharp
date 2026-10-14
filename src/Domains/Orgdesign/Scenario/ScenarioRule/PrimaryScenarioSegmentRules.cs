namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary scenario segment must pass. Check returns its problems, or none.</summary>
public interface IPrimaryScenarioSegmentRule
{
    List<string> Check(PrimaryScenarioSegment item);
}

public sealed record PrimaryScenarioSegmentCodeLengthRule(int MaxLength) : IPrimaryScenarioSegmentRule
{
    public List<string> Check(PrimaryScenarioSegment item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioSegmentAllowedTitleRule(List<string> Allowed) : IPrimaryScenarioSegmentRule
{
    public List<string> Check(PrimaryScenarioSegment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is stale" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioSegmentRequiredCodeRule(string Message) : IPrimaryScenarioSegmentRule
{
    public List<string> Check(PrimaryScenarioSegment item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryScenarioSegmentRevisionRangeRule(int Min, int Max) : IPrimaryScenarioSegmentRule
{
    public List<string> Check(PrimaryScenarioSegment item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryScenarioSegmentDurationMinutesRangeRule(int Min, int Max) : IPrimaryScenarioSegmentRule
{
    public List<string> Check(PrimaryScenarioSegment item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary scenario segment.</summary>
public sealed record PrimaryScenarioSegmentValidator(List<IPrimaryScenarioSegmentRule> Rules)
{
    public List<string> Validate(PrimaryScenarioSegment item)
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

    public void RequireValid(PrimaryScenarioSegment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryScenarioSegment> items)
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

/// <summary>Functions of primary scenario segment rules.</summary>
public static class PrimaryScenarioSegmentRules
{
    public static PrimaryScenarioSegmentValidator DefaultPrimaryScenarioSegmentValidator()
    {
        return new PrimaryScenarioSegmentValidator(
            new List<IPrimaryScenarioSegmentRule>
            {
                new PrimaryScenarioSegmentCodeLengthRule(3600),
                new PrimaryScenarioSegmentAllowedTitleRule(new List<string> { "manual", "pending" }),
                new PrimaryScenarioSegmentRequiredCodeRule("primary scenario segment needs this field"),
                new PrimaryScenarioSegmentRevisionRangeRule(16, 1000),
                new PrimaryScenarioSegmentDurationMinutesRangeRule(120, 3600),
            });
    }
}
