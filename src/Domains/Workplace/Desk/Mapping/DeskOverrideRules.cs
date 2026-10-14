namespace Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>A rule a desk override must pass. Check returns its problems, or none.</summary>
public interface IDeskOverrideRule
{
    List<string> Check(DeskOverride item);
}

public sealed record DeskOverrideExternalRefLengthRule(int MaxLength) : IDeskOverrideRule
{
    public List<string> Check(DeskOverride item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeskOverrideVersionRangeRule(int Min, int Max) : IDeskOverrideRule
{
    public List<string> Check(DeskOverride item)
    {
        var valueValue = item.Version;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"version {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"version {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeskOverrideLabelLengthRule(int MaxLength) : IDeskOverrideRule
{
    public List<string> Check(DeskOverride item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a desk override.</summary>
public sealed record DeskOverrideValidator(List<IDeskOverrideRule> Rules)
{
    public List<string> Validate(DeskOverride item)
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

    public void RequireValid(DeskOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeskOverride> items)
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

/// <summary>Functions of desk override rules.</summary>
public static class DeskOverrideRules
{
    public static DeskOverrideValidator DefaultDeskOverrideValidator()
    {
        return new DeskOverrideValidator(
            new List<IDeskOverrideRule>
            {
                new DeskOverrideExternalRefLengthRule(3600),
                new DeskOverrideVersionRangeRule(80, 255),
                new DeskOverrideLabelLengthRule(1000),
            });
    }
}
