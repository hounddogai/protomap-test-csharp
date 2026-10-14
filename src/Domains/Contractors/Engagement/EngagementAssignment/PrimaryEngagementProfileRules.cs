namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

/// <summary>A rule a primary engagement profile must pass. Check returns its problems, or none.</summary>
public interface IPrimaryEngagementProfileRule
{
    List<string> Check(PrimaryEngagementProfile item);
}

public sealed record PrimaryEngagementProfileCodePrefixRule(string Prefix) : IPrimaryEngagementProfileRule
{
    public List<string> Check(PrimaryEngagementProfile item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementProfileBudgetRangeRule(double Min, double Max) : IPrimaryEngagementProfileRule
{
    public List<string> Check(PrimaryEngagementProfile item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimaryEngagementProfileEndsAtRangeRule(int Min, int Max) : IPrimaryEngagementProfileRule
{
    public List<string> Check(PrimaryEngagementProfile item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementProfileRequiredCodeRule(string Message) : IPrimaryEngagementProfileRule
{
    public List<string> Check(PrimaryEngagementProfile item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryEngagementProfileCodeLengthRule(int MaxLength) : IPrimaryEngagementProfileRule
{
    public List<string> Check(PrimaryEngagementProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary engagement profile.</summary>
public sealed record PrimaryEngagementProfileValidator(List<IPrimaryEngagementProfileRule> Rules)
{
    public List<string> Validate(PrimaryEngagementProfile item)
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

    public void RequireValid(PrimaryEngagementProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryEngagementProfile> items)
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

/// <summary>Functions of primary engagement profile rules.</summary>
public static class PrimaryEngagementProfileRules
{
    public static PrimaryEngagementProfileValidator DefaultPrimaryEngagementProfileValidator()
    {
        return new PrimaryEngagementProfileValidator(
            new List<IPrimaryEngagementProfileRule>
            {
                new PrimaryEngagementProfileCodePrefixRule("ACM"),
                new PrimaryEngagementProfileBudgetRangeRule(3.0, 1231.6),
                new PrimaryEngagementProfileEndsAtRangeRule(80, 86400),
                new PrimaryEngagementProfileRequiredCodeRule("primary engagement profile needs an entry"),
                new PrimaryEngagementProfileCodeLengthRule(16),
            });
    }
}
