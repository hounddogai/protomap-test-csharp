namespace Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>A rule a rolling position bundle must pass. Check returns its problems, or none.</summary>
public interface IRollingPositionBundleRule
{
    List<string> Check(RollingPositionBundle item);
}

public sealed record RollingPositionBundleStartsAtRangeRule(int Min, int Max) : IRollingPositionBundleRule
{
    public List<string> Check(RollingPositionBundle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionBundleWindowRule(int MaxSpan) : IRollingPositionBundleRule
{
    public List<string> Check(RollingPositionBundle item)
    {
        if (item.StartsAt > item.EndsAt)
        {
            return new List<string> { "starts at is after ends at" };
        }
        if (MaxSpan > 0 && item.EndsAt - item.StartsAt > MaxSpan)
        {
            return new List<string> { $"spans more than {MaxSpan} seconds" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionBundleCodeLengthRule(int MaxLength) : IRollingPositionBundleRule
{
    public List<string> Check(RollingPositionBundle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingPositionBundleRevisionRangeRule(int Min, int Max) : IRollingPositionBundleRule
{
    public List<string> Check(RollingPositionBundle item)
    {
        var valueValue = item.Revision;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"revision {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling position bundle.</summary>
public sealed record RollingPositionBundleValidator(List<IRollingPositionBundleRule> Rules)
{
    public List<string> Validate(RollingPositionBundle item)
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

    public void RequireValid(RollingPositionBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingPositionBundle> items)
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

/// <summary>Functions of rolling position bundle rules.</summary>
public static class RollingPositionBundleRules
{
    public static RollingPositionBundleValidator DefaultRollingPositionBundleValidator()
    {
        return new RollingPositionBundleValidator(
            new List<IRollingPositionBundleRule>
            {
                new RollingPositionBundleStartsAtRangeRule(3600, 86400),
                new RollingPositionBundleWindowRule(1000),
                new RollingPositionBundleCodeLengthRule(32),
                new RollingPositionBundleRevisionRangeRule(120, 1000),
            });
    }
}
