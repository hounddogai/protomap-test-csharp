namespace Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>A rule a town hall profile must pass. Check returns its problems, or none.</summary>
public interface ITownHallProfileRule
{
    List<string> Check(TownHallProfile item);
}

public sealed record TownHallProfileCodePrefixRule(string Prefix) : ITownHallProfileRule
{
    public List<string> Check(TownHallProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProfileMinimumRangeRule(double Min, double Max) : ITownHallProfileRule
{
    public List<string> Check(TownHallProfile item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProfileDepartmentLengthRule(int MaxLength) : ITownHallProfileRule
{
    public List<string> Check(TownHallProfile item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TownHallProfileRequiredDepartmentRule(string Message) : ITownHallProfileRule
{
    public List<string> Check(TownHallProfile item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a town hall profile.</summary>
public sealed record TownHallProfileValidator(List<ITownHallProfileRule> Rules)
{
    public List<string> Validate(TownHallProfile item)
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

    public void RequireValid(TownHallProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TownHallProfile> items)
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

/// <summary>Functions of town hall profile rules.</summary>
public static class TownHallProfileRules
{
    public static TownHallProfileValidator DefaultTownHallProfileValidator()
    {
        return new TownHallProfileValidator(
            new List<ITownHallProfileRule>
            {
                new TownHallProfileCodePrefixRule("Q"),
                new TownHallProfileMinimumRangeRule(0.2, 1885.7),
                new TownHallProfileDepartmentLengthRule(80),
                new TownHallProfileRequiredDepartmentRule("town hall profile needs a value"),
            });
    }
}
