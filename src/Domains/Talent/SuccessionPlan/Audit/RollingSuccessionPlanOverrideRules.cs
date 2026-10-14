namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>A rule a rolling succession plan override must pass. Check returns its problems, or none.</summary>
public interface IRollingSuccessionPlanOverrideRule
{
    List<string> Check(RollingSuccessionPlanOverride item);
}

public sealed record RollingSuccessionPlanOverrideRegionLengthRule(int MaxLength) : IRollingSuccessionPlanOverrideRule
{
    public List<string> Check(RollingSuccessionPlanOverride item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanOverrideMaximumRangeRule(double Min, double Max) : IRollingSuccessionPlanOverrideRule
{
    public List<string> Check(RollingSuccessionPlanOverride item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSuccessionPlanOverrideCodeLengthRule(int MaxLength) : IRollingSuccessionPlanOverrideRule
{
    public List<string> Check(RollingSuccessionPlanOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanOverrideAmountRangeRule(double Min, double Max) : IRollingSuccessionPlanOverrideRule
{
    public List<string> Check(RollingSuccessionPlanOverride item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RollingSuccessionPlanOverrideCodePrefixRule(string Prefix) : IRollingSuccessionPlanOverrideRule
{
    public List<string> Check(RollingSuccessionPlanOverride item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling succession plan override.</summary>
public sealed record RollingSuccessionPlanOverrideValidator(List<IRollingSuccessionPlanOverrideRule> Rules)
{
    public List<string> Validate(RollingSuccessionPlanOverride item)
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

    public void RequireValid(RollingSuccessionPlanOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSuccessionPlanOverride> items)
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

/// <summary>Functions of rolling succession plan override rules.</summary>
public static class RollingSuccessionPlanOverrideRules
{
    public static RollingSuccessionPlanOverrideValidator DefaultRollingSuccessionPlanOverrideValidator()
    {
        return new RollingSuccessionPlanOverrideValidator(
            new List<IRollingSuccessionPlanOverrideRule>
            {
                new RollingSuccessionPlanOverrideRegionLengthRule(3600),
                new RollingSuccessionPlanOverrideMaximumRangeRule(5.9, 3656.4),
                new RollingSuccessionPlanOverrideCodeLengthRule(3600),
                new RollingSuccessionPlanOverrideAmountRangeRule(6.5, 4737.2),
                new RollingSuccessionPlanOverrideCodePrefixRule("X"),
            });
    }
}
