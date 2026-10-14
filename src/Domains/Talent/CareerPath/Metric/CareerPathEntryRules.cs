namespace Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>A rule a career path entry must pass. Check returns its problems, or none.</summary>
public interface ICareerPathEntryRule
{
    List<string> Check(CareerPathEntry item);
}

public sealed record CareerPathEntryLeaveDaysRangeRule(int Min, int Max) : ICareerPathEntryRule
{
    public List<string> Check(CareerPathEntry item)
    {
        var valueValue = item.LeaveDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"leave days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathEntryRequiredNameRule(string Message) : ICareerPathEntryRule
{
    public List<string> Check(CareerPathEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathEntryRequiredTitleRule(string Message) : ICareerPathEntryRule
{
    public List<string> Check(CareerPathEntry item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path entry.</summary>
public sealed record CareerPathEntryValidator(List<ICareerPathEntryRule> Rules)
{
    public List<string> Validate(CareerPathEntry item)
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

    public void RequireValid(CareerPathEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathEntry> items)
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

/// <summary>Functions of career path entry rules.</summary>
public static class CareerPathEntryRules
{
    public static CareerPathEntryValidator DefaultCareerPathEntryValidator()
    {
        return new CareerPathEntryValidator(
            new List<ICareerPathEntryRule>
            {
                new CareerPathEntryLeaveDaysRangeRule(1000, 3600),
                new CareerPathEntryRequiredNameRule("career path entry needs an entry"),
                new CareerPathEntryRequiredTitleRule("career path entry needs this field"),
            });
    }
}
