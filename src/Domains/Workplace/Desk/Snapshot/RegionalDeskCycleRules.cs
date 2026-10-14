namespace Acme.Workforce.Workplace.Desk.Snapshot;

/// <summary>A rule a regional desk cycle must pass. Check returns its problems, or none.</summary>
public interface IRegionalDeskCycleRule
{
    List<string> Check(RegionalDeskCycle item);
}

public sealed record RegionalDeskCycleLocationLengthRule(int MaxLength) : IRegionalDeskCycleRule
{
    public List<string> Check(RegionalDeskCycle item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskCycleRequiredNameRule(string Message) : IRegionalDeskCycleRule
{
    public List<string> Check(RegionalDeskCycle item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RegionalDeskCycleRatioRangeRule(double Min, double Max) : IRegionalDeskCycleRule
{
    public List<string> Check(RegionalDeskCycle item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a regional desk cycle.</summary>
public sealed record RegionalDeskCycleValidator(List<IRegionalDeskCycleRule> Rules)
{
    public List<string> Validate(RegionalDeskCycle item)
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

    public void RequireValid(RegionalDeskCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RegionalDeskCycle> items)
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

/// <summary>Functions of regional desk cycle rules.</summary>
public static class RegionalDeskCycleRules
{
    public static RegionalDeskCycleValidator DefaultRegionalDeskCycleValidator()
    {
        return new RegionalDeskCycleValidator(
            new List<IRegionalDeskCycleRule>
            {
                new RegionalDeskCycleLocationLengthRule(16),
                new RegionalDeskCycleRequiredNameRule("regional desk cycle needs a value"),
                new RegionalDeskCycleRatioRangeRule(3.9, 3887.2),
            });
    }
}
