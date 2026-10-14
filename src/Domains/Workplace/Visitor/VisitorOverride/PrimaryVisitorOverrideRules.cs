namespace Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>A rule a primary visitor override must pass. Check returns its problems, or none.</summary>
public interface IPrimaryVisitorOverrideRule
{
    List<string> Check(PrimaryVisitorOverride item);
}

public sealed record PrimaryVisitorOverrideDurationMinutesRangeRule(int Min, int Max) : IPrimaryVisitorOverrideRule
{
    public List<string> Check(PrimaryVisitorOverride item)
    {
        var valueValue = item.DurationMinutes;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"duration minutes {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorOverrideMinimumRangeRule(double Min, double Max) : IPrimaryVisitorOverrideRule
{
    public List<string> Check(PrimaryVisitorOverride item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorOverrideNameLengthRule(int MaxLength) : IPrimaryVisitorOverrideRule
{
    public List<string> Check(PrimaryVisitorOverride item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryVisitorOverrideCreatedAtRangeRule(int Min, int Max) : IPrimaryVisitorOverrideRule
{
    public List<string> Check(PrimaryVisitorOverride item)
    {
        var valueValue = item.CreatedAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"created at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary visitor override.</summary>
public sealed record PrimaryVisitorOverrideValidator(List<IPrimaryVisitorOverrideRule> Rules)
{
    public List<string> Validate(PrimaryVisitorOverride item)
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

    public void RequireValid(PrimaryVisitorOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryVisitorOverride> items)
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

/// <summary>Functions of primary visitor override rules.</summary>
public static class PrimaryVisitorOverrideRules
{
    public static PrimaryVisitorOverrideValidator DefaultPrimaryVisitorOverrideValidator()
    {
        return new PrimaryVisitorOverrideValidator(
            new List<IPrimaryVisitorOverrideRule>
            {
                new PrimaryVisitorOverrideDurationMinutesRangeRule(80, 1000),
                new PrimaryVisitorOverrideMinimumRangeRule(2.2, 1399.4),
                new PrimaryVisitorOverrideNameLengthRule(80),
                new PrimaryVisitorOverrideCreatedAtRangeRule(32, 255),
            });
    }
}
