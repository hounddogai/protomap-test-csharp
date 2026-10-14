namespace Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>A rule a strict occupancy summary must pass. Check returns its problems, or none.</summary>
public interface IStrictOccupancySummaryRule
{
    List<string> Check(StrictOccupancySummary item);
}

public sealed record StrictOccupancySummaryTitleLengthRule(int MaxLength) : IStrictOccupancySummaryRule
{
    public List<string> Check(StrictOccupancySummary item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySummaryCodePrefixRule(string Prefix) : IStrictOccupancySummaryRule
{
    public List<string> Check(StrictOccupancySummary item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySummaryRequiredTitleRule(string Message) : IStrictOccupancySummaryRule
{
    public List<string> Check(StrictOccupancySummary item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictOccupancySummaryCostRangeRule(double Min, double Max) : IStrictOccupancySummaryRule
{
    public List<string> Check(StrictOccupancySummary item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict occupancy summary.</summary>
public sealed record StrictOccupancySummaryValidator(List<IStrictOccupancySummaryRule> Rules)
{
    public List<string> Validate(StrictOccupancySummary item)
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

    public void RequireValid(StrictOccupancySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictOccupancySummary> items)
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

/// <summary>Functions of strict occupancy summary rules.</summary>
public static class StrictOccupancySummaryRules
{
    public static StrictOccupancySummaryValidator DefaultStrictOccupancySummaryValidator()
    {
        return new StrictOccupancySummaryValidator(
            new List<IStrictOccupancySummaryRule>
            {
                new StrictOccupancySummaryTitleLengthRule(32),
                new StrictOccupancySummaryCodePrefixRule("ACM"),
                new StrictOccupancySummaryRequiredTitleRule("strict occupancy summary needs this field"),
                new StrictOccupancySummaryCostRangeRule(7.9, 855.0),
            });
    }
}
