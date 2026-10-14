namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

/// <summary>A rule a equity grant entry must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantEntryRule
{
    List<string> Check(EquityGrantEntry item);
}

public sealed record EquityGrantEntryBudgetRangeRule(double Min, double Max) : IEquityGrantEntryRule
{
    public List<string> Check(EquityGrantEntry item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantEntryRevisionRangeRule(int Min, int Max) : IEquityGrantEntryRule
{
    public List<string> Check(EquityGrantEntry item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantEntryRequiredCodeRule(string Message) : IEquityGrantEntryRule
{
    public List<string> Check(EquityGrantEntry item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant entry.</summary>
public sealed record EquityGrantEntryValidator(List<IEquityGrantEntryRule> Rules)
{
    public List<string> Validate(EquityGrantEntry item)
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

    public void RequireValid(EquityGrantEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantEntry> items)
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

/// <summary>Functions of equity grant entry rules.</summary>
public static class EquityGrantEntryRules
{
    public static EquityGrantEntryValidator DefaultEquityGrantEntryValidator()
    {
        return new EquityGrantEntryValidator(
            new List<IEquityGrantEntryRule>
            {
                new EquityGrantEntryBudgetRangeRule(9.5, 1464.4),
                new EquityGrantEntryRevisionRangeRule(80, 1000),
                new EquityGrantEntryRequiredCodeRule("equity grant entry needs an entry"),
            });
    }
}
