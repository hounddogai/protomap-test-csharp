namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a manual access badge budget must pass. Check returns its problems, or none.</summary>
public interface IManualAccessBadgeBudgetRule
{
    List<string> Check(ManualAccessBadgeBudget item);
}

public sealed record ManualAccessBadgeBudgetLimitRangeRule(int Min, int Max) : IManualAccessBadgeBudgetRule
{
    public List<string> Check(ManualAccessBadgeBudget item)
    {
        var valueValue = item.Limit;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"limit {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"limit {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualAccessBadgeBudgetChecksumLengthRule(int MaxLength) : IManualAccessBadgeBudgetRule
{
    public List<string> Check(ManualAccessBadgeBudget item)
    {
        var size = item.Checksum.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"checksum is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeBudgetRequiredChecksumRule(string Message) : IManualAccessBadgeBudgetRule
{
    public List<string> Check(ManualAccessBadgeBudget item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeBudgetAllowedTitleRule(List<string> Allowed) : IManualAccessBadgeBudgetRule
{
    public List<string> Check(ManualAccessBadgeBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ManualAccessBadgeBudgetCodePrefixRule(string Prefix) : IManualAccessBadgeBudgetRule
{
    public List<string> Check(ManualAccessBadgeBudget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual access badge budget.</summary>
public sealed record ManualAccessBadgeBudgetValidator(List<IManualAccessBadgeBudgetRule> Rules)
{
    public List<string> Validate(ManualAccessBadgeBudget item)
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

    public void RequireValid(ManualAccessBadgeBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualAccessBadgeBudget> items)
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

/// <summary>Functions of manual access badge budget rules.</summary>
public static class ManualAccessBadgeBudgetRules
{
    public static ManualAccessBadgeBudgetValidator DefaultManualAccessBadgeBudgetValidator()
    {
        return new ManualAccessBadgeBudgetValidator(
            new List<IManualAccessBadgeBudgetRule>
            {
                new ManualAccessBadgeBudgetLimitRangeRule(16, 64),
                new ManualAccessBadgeBudgetChecksumLengthRule(16),
                new ManualAccessBadgeBudgetRequiredChecksumRule("manual access badge budget needs an entry"),
                new ManualAccessBadgeBudgetAllowedTitleRule(new List<string> { "weighted" }),
                new ManualAccessBadgeBudgetCodePrefixRule("ACM"),
            });
    }
}
