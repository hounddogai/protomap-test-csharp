namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

/// <summary>A rule a tiered pay run milestone must pass. Check returns its problems, or none.</summary>
public interface ITieredPayRunMilestoneRule
{
    List<string> Check(TieredPayRunMilestone item);
}

public sealed record TieredPayRunMilestoneCodePrefixRule(string Prefix) : ITieredPayRunMilestoneRule
{
    public List<string> Check(TieredPayRunMilestone item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunMilestoneTenureMonthsRangeRule(int Min, int Max) : ITieredPayRunMilestoneRule
{
    public List<string> Check(TieredPayRunMilestone item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TieredPayRunMilestoneLabelLengthRule(int MaxLength) : ITieredPayRunMilestoneRule
{
    public List<string> Check(TieredPayRunMilestone item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tiered pay run milestone.</summary>
public sealed record TieredPayRunMilestoneValidator(List<ITieredPayRunMilestoneRule> Rules)
{
    public List<string> Validate(TieredPayRunMilestone item)
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

    public void RequireValid(TieredPayRunMilestone item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TieredPayRunMilestone> items)
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

/// <summary>Functions of tiered pay run milestone rules.</summary>
public static class TieredPayRunMilestoneRules
{
    public static TieredPayRunMilestoneValidator DefaultTieredPayRunMilestoneValidator()
    {
        return new TieredPayRunMilestoneValidator(
            new List<ITieredPayRunMilestoneRule>
            {
                new TieredPayRunMilestoneCodePrefixRule("REF"),
                new TieredPayRunMilestoneTenureMonthsRangeRule(32, 64),
                new TieredPayRunMilestoneLabelLengthRule(1000),
            });
    }
}
