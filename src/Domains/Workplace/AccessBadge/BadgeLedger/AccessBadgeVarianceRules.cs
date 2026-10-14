namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>A rule a access badge variance must pass. Check returns its problems, or none.</summary>
public interface IAccessBadgeVarianceRule
{
    List<string> Check(AccessBadgeVariance item);
}

public sealed record AccessBadgeVarianceWeightRangeRule(double Min, double Max) : IAccessBadgeVarianceRule
{
    public List<string> Check(AccessBadgeVariance item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record AccessBadgeVarianceLocationLengthRule(int MaxLength) : IAccessBadgeVarianceRule
{
    public List<string> Check(AccessBadgeVariance item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeVarianceWindowDaysRangeRule(int Min, int Max) : IAccessBadgeVarianceRule
{
    public List<string> Check(AccessBadgeVariance item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeVarianceMaximumRangeRule(double Min, double Max) : IAccessBadgeVarianceRule
{
    public List<string> Check(AccessBadgeVariance item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AccessBadgeVarianceRequiredNameRule(string Message) : IAccessBadgeVarianceRule
{
    public List<string> Check(AccessBadgeVariance item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a access badge variance.</summary>
public sealed record AccessBadgeVarianceValidator(List<IAccessBadgeVarianceRule> Rules)
{
    public List<string> Validate(AccessBadgeVariance item)
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

    public void RequireValid(AccessBadgeVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AccessBadgeVariance> items)
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

/// <summary>Functions of access badge variance rules.</summary>
public static class AccessBadgeVarianceRules
{
    public static AccessBadgeVarianceValidator DefaultAccessBadgeVarianceValidator()
    {
        return new AccessBadgeVarianceValidator(
            new List<IAccessBadgeVarianceRule>
            {
                new AccessBadgeVarianceWeightRangeRule(9.0, 3987.5),
                new AccessBadgeVarianceLocationLengthRule(1000),
                new AccessBadgeVarianceWindowDaysRangeRule(32, 64),
                new AccessBadgeVarianceMaximumRangeRule(3.9, 4619.3),
                new AccessBadgeVarianceRequiredNameRule("access badge variance needs a value"),
            });
    }
}
