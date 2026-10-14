namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>A rule a default floor plan baseline must pass. Check returns its problems, or none.</summary>
public interface IDefaultFloorPlanBaselineRule
{
    List<string> Check(DefaultFloorPlanBaseline item);
}

public sealed record DefaultFloorPlanBaselineCodePrefixRule(string Prefix) : IDefaultFloorPlanBaselineRule
{
    public List<string> Check(DefaultFloorPlanBaseline item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanBaselineCodeLengthRule(int MaxLength) : IDefaultFloorPlanBaselineRule
{
    public List<string> Check(DefaultFloorPlanBaseline item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanBaselineRetriesRangeRule(int Min, int Max) : IDefaultFloorPlanBaselineRule
{
    public List<string> Check(DefaultFloorPlanBaseline item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanBaselineRequiredCodeRule(string Message) : IDefaultFloorPlanBaselineRule
{
    public List<string> Check(DefaultFloorPlanBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultFloorPlanBaselineNotesLengthRule(int MaxLength) : IDefaultFloorPlanBaselineRule
{
    public List<string> Check(DefaultFloorPlanBaseline item)
    {
        var size = item.Notes.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"notes is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default floor plan baseline.</summary>
public sealed record DefaultFloorPlanBaselineValidator(List<IDefaultFloorPlanBaselineRule> Rules)
{
    public List<string> Validate(DefaultFloorPlanBaseline item)
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

    public void RequireValid(DefaultFloorPlanBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultFloorPlanBaseline> items)
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

/// <summary>Functions of default floor plan baseline rules.</summary>
public static class DefaultFloorPlanBaselineRules
{
    public static DefaultFloorPlanBaselineValidator DefaultDefaultFloorPlanBaselineValidator()
    {
        return new DefaultFloorPlanBaselineValidator(
            new List<IDefaultFloorPlanBaselineRule>
            {
                new DefaultFloorPlanBaselineCodePrefixRule("REF"),
                new DefaultFloorPlanBaselineCodeLengthRule(16),
                new DefaultFloorPlanBaselineRetriesRangeRule(64, 86400),
                new DefaultFloorPlanBaselineRequiredCodeRule("default floor plan baseline needs a value"),
                new DefaultFloorPlanBaselineNotesLengthRule(80),
            });
    }
}
