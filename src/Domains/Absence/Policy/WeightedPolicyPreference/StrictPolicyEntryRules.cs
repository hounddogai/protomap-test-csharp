namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>A rule a strict policy entry must pass. Check returns its problems, or none.</summary>
public interface IStrictPolicyEntryRule
{
    List<string> Check(StrictPolicyEntry item);
}

public sealed record StrictPolicyEntryDiscountRangeRule(double Min, double Max) : IStrictPolicyEntryRule
{
    public List<string> Check(StrictPolicyEntry item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record StrictPolicyEntryRequiredNameRule(string Message) : IStrictPolicyEntryRule
{
    public List<string> Check(StrictPolicyEntry item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record StrictPolicyEntryRetriesRangeRule(int Min, int Max) : IStrictPolicyEntryRule
{
    public List<string> Check(StrictPolicyEntry item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a strict policy entry.</summary>
public sealed record StrictPolicyEntryValidator(List<IStrictPolicyEntryRule> Rules)
{
    public List<string> Validate(StrictPolicyEntry item)
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

    public void RequireValid(StrictPolicyEntry item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<StrictPolicyEntry> items)
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

/// <summary>Functions of strict policy entry rules.</summary>
public static class StrictPolicyEntryRules
{
    public static StrictPolicyEntryValidator DefaultStrictPolicyEntryValidator()
    {
        return new StrictPolicyEntryValidator(
            new List<IStrictPolicyEntryRule>
            {
                new StrictPolicyEntryDiscountRangeRule(7.1, 1231.7),
                new StrictPolicyEntryRequiredNameRule("strict policy entry needs an entry"),
                new StrictPolicyEntryRetriesRangeRule(32, 3600),
            });
    }
}
