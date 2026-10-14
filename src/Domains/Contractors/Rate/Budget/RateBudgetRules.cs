namespace Acme.Workforce.Contractors.Rate.Budget;

/// <summary>A rule a rate budget must pass. Check returns its problems, or none.</summary>
public interface IRateBudgetRule
{
    List<string> Check(RateBudget item);
}

public sealed record RateBudgetVersionRangeRule(int Min, int Max) : IRateBudgetRule
{
    public List<string> Check(RateBudget item)
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

public sealed record RateBudgetThresholdRangeRule(double Min, double Max) : IRateBudgetRule
{
    public List<string> Check(RateBudget item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RateBudgetNameLengthRule(int MaxLength) : IRateBudgetRule
{
    public List<string> Check(RateBudget item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RateBudgetMinimumRangeRule(double Min, double Max) : IRateBudgetRule
{
    public List<string> Check(RateBudget item)
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

/// <summary>Runs every rule of a rate budget.</summary>
public sealed record RateBudgetValidator(List<IRateBudgetRule> Rules)
{
    public List<string> Validate(RateBudget item)
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

    public void RequireValid(RateBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RateBudget> items)
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

/// <summary>Functions of rate budget rules.</summary>
public static class RateBudgetRules
{
    public static RateBudgetValidator DefaultRateBudgetValidator()
    {
        return new RateBudgetValidator(
            new List<IRateBudgetRule>
            {
                new RateBudgetVersionRangeRule(16, 80),
                new RateBudgetThresholdRangeRule(6.7, 282.6),
                new RateBudgetNameLengthRule(1000),
                new RateBudgetMinimumRangeRule(7.5, 973.9),
            });
    }
}
