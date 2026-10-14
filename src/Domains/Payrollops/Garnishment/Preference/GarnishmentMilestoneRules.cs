namespace Acme.Workforce.Payrollops.Garnishment.Preference;

/// <summary>A rule a garnishment milestone must pass. Check returns its problems, or none.</summary>
public interface IGarnishmentMilestoneRule
{
    List<string> Check(GarnishmentMilestone item);
}

public sealed record GarnishmentMilestoneLeaveDaysRangeRule(int Min, int Max) : IGarnishmentMilestoneRule
{
    public List<string> Check(GarnishmentMilestone item)
    {
        var valueValue = item.LeaveDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"leave days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"leave days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record GarnishmentMilestoneCurrencyLengthRule(int MaxLength) : IGarnishmentMilestoneRule
{
    public List<string> Check(GarnishmentMilestone item)
    {
        var size = item.Currency.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"currency is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMilestoneRequiredCurrencyRule(string Message) : IGarnishmentMilestoneRule
{
    public List<string> Check(GarnishmentMilestone item)
    {
        if (item.Currency.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record GarnishmentMilestoneCodePrefixRule(string Prefix) : IGarnishmentMilestoneRule
{
    public List<string> Check(GarnishmentMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a garnishment milestone.</summary>
public sealed record GarnishmentMilestoneValidator(List<IGarnishmentMilestoneRule> Rules)
{
    public List<string> Validate(GarnishmentMilestone item)
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

    public void RequireValid(GarnishmentMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GarnishmentMilestone> items)
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

/// <summary>Functions of garnishment milestone rules.</summary>
public static class GarnishmentMilestoneRules
{
    public static GarnishmentMilestoneValidator DefaultGarnishmentMilestoneValidator()
    {
        return new GarnishmentMilestoneValidator(
            new List<IGarnishmentMilestoneRule>
            {
                new GarnishmentMilestoneLeaveDaysRangeRule(32, 80),
                new GarnishmentMilestoneCurrencyLengthRule(255),
                new GarnishmentMilestoneRequiredCurrencyRule("garnishment milestone needs an entry"),
                new GarnishmentMilestoneCodePrefixRule("ACM"),
            });
    }
}
