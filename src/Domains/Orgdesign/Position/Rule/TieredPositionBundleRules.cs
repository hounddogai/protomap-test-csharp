namespace Acme.Workforce.Orgdesign.Position.Rule;

/// <summary>A rule a tiered position bundle must pass. Check returns its problems, or none.</summary>
public interface ITieredPositionBundleRule
{
    List<string> Check(TieredPositionBundle item);
}

public sealed record TieredPositionBundleEmployeeIdLengthRule(int MaxLength) : ITieredPositionBundleRule
{
    public List<string> Check(TieredPositionBundle item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionBundleRequiredCodeRule(string Message) : ITieredPositionBundleRule
{
    public List<string> Check(TieredPositionBundle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionBundleMinimumRangeRule(double Min, double Max) : ITieredPositionBundleRule
{
    public List<string> Check(TieredPositionBundle item)
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

public sealed record TieredPositionBundleNameLengthRule(int MaxLength) : ITieredPositionBundleRule
{
    public List<string> Check(TieredPositionBundle item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPositionBundleRequiredSegmentRule(string Message) : ITieredPositionBundleRule
{
    public List<string> Check(TieredPositionBundle item)
    {
        if (item.Segment.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered position bundle.</summary>
public sealed record TieredPositionBundleValidator(List<ITieredPositionBundleRule> Rules)
{
    public List<string> Validate(TieredPositionBundle item)
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

    public void RequireValid(TieredPositionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPositionBundle> items)
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

/// <summary>Functions of tiered position bundle rules.</summary>
public static class TieredPositionBundleRules
{
    public static TieredPositionBundleValidator DefaultTieredPositionBundleValidator()
    {
        return new TieredPositionBundleValidator(
            new List<ITieredPositionBundleRule>
            {
                new TieredPositionBundleEmployeeIdLengthRule(80),
                new TieredPositionBundleRequiredCodeRule("tiered position bundle needs this field"),
                new TieredPositionBundleMinimumRangeRule(0.8, 1961.4),
                new TieredPositionBundleNameLengthRule(16),
                new TieredPositionBundleRequiredSegmentRule("tiered position bundle needs an entry"),
            });
    }
}
