namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>A rule a tiered desk cycle must pass. Check returns its problems, or none.</summary>
public interface ITieredDeskCycleRule
{
    List<string> Check(TieredDeskCycle item);
}

public sealed record TieredDeskCycleRetriesRangeRule(int Min, int Max) : ITieredDeskCycleRule
{
    public List<string> Check(TieredDeskCycle item)
    {
        var valueValue = item.Retries;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"retries {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskCycleLabelLengthRule(int MaxLength) : ITieredDeskCycleRule
{
    public List<string> Check(TieredDeskCycle item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskCycleContactEmailLengthRule(int MaxLength) : ITieredDeskCycleRule
{
    public List<string> Check(TieredDeskCycle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TieredDeskCycleRequiredLabelRule(string Message) : ITieredDeskCycleRule
{
    public List<string> Check(TieredDeskCycle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered desk cycle.</summary>
public sealed record TieredDeskCycleValidator(List<ITieredDeskCycleRule> Rules)
{
    public List<string> Validate(TieredDeskCycle item)
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

    public void RequireValid(TieredDeskCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredDeskCycle> items)
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

/// <summary>Functions of tiered desk cycle rules.</summary>
public static class TieredDeskCycleRules
{
    public static TieredDeskCycleValidator DefaultTieredDeskCycleValidator()
    {
        return new TieredDeskCycleValidator(
            new List<ITieredDeskCycleRule>
            {
                new TieredDeskCycleRetriesRangeRule(255, 86400),
                new TieredDeskCycleLabelLengthRule(120),
                new TieredDeskCycleContactEmailLengthRule(64),
                new TieredDeskCycleRequiredLabelRule("tiered desk cycle needs an entry"),
            });
    }
}
