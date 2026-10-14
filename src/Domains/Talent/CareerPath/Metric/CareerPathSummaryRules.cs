namespace Acme.Workforce.Talent.CareerPath.Metric;

/// <summary>A rule a career path summary must pass. Check returns its problems, or none.</summary>
public interface ICareerPathSummaryRule
{
    List<string> Check(CareerPathSummary item);
}

public sealed record CareerPathSummaryLocationLengthRule(int MaxLength) : ICareerPathSummaryRule
{
    public List<string> Check(CareerPathSummary item)
    {
        var size = item.Location.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"location is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSummaryEndsAtRangeRule(int Min, int Max) : ICareerPathSummaryRule
{
    public List<string> Check(CareerPathSummary item)
    {
        var valueValue = item.EndsAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ends at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ends at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record CareerPathSummaryRequiredLocationRule(string Message) : ICareerPathSummaryRule
{
    public List<string> Check(CareerPathSummary item)
    {
        if (item.Location.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record CareerPathSummaryPinnedGuardRule(int Limit) : ICareerPathSummaryRule
{
    public List<string> Check(CareerPathSummary item)
    {
        if (item.Pinned && item.EndsAt > Limit)
        {
            return new List<string> { $"ends at {item.EndsAt} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a career path summary.</summary>
public sealed record CareerPathSummaryValidator(List<ICareerPathSummaryRule> Rules)
{
    public List<string> Validate(CareerPathSummary item)
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

    public void RequireValid(CareerPathSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<CareerPathSummary> items)
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

/// <summary>Functions of career path summary rules.</summary>
public static class CareerPathSummaryRules
{
    public static CareerPathSummaryValidator DefaultCareerPathSummaryValidator()
    {
        return new CareerPathSummaryValidator(
            new List<ICareerPathSummaryRule>
            {
                new CareerPathSummaryLocationLengthRule(32),
                new CareerPathSummaryEndsAtRangeRule(64, 80),
                new CareerPathSummaryRequiredLocationRule("career path summary needs this field"),
                new CareerPathSummaryPinnedGuardRule(16),
            });
    }
}
