namespace Acme.Workforce.Contractors.Offboarding.Notice;

/// <summary>A rule a rolling offboarding rollup must pass. Check returns its problems, or none.</summary>
public interface IRollingOffboardingRollupRule
{
    List<string> Check(RollingOffboardingRollup item);
}

public sealed record RollingOffboardingRollupCodeLengthRule(int MaxLength) : IRollingOffboardingRollupRule
{
    public List<string> Check(RollingOffboardingRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingOffboardingRollupThresholdRangeRule(double Min, double Max) : IRollingOffboardingRollupRule
{
    public List<string> Check(RollingOffboardingRollup item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingOffboardingRollupRequiredLabelRule(string Message) : IRollingOffboardingRollupRule
{
    public List<string> Check(RollingOffboardingRollup item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingOffboardingRollupCostCenterLengthRule(int MaxLength) : IRollingOffboardingRollupRule
{
    public List<string> Check(RollingOffboardingRollup item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingOffboardingRollupCodePrefixRule(string Prefix) : IRollingOffboardingRollupRule
{
    public List<string> Check(RollingOffboardingRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling offboarding rollup.</summary>
public sealed record RollingOffboardingRollupValidator(List<IRollingOffboardingRollupRule> Rules)
{
    public List<string> Validate(RollingOffboardingRollup item)
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

    public void RequireValid(RollingOffboardingRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingOffboardingRollup> items)
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

/// <summary>Functions of rolling offboarding rollup rules.</summary>
public static class RollingOffboardingRollupRules
{
    public static RollingOffboardingRollupValidator DefaultRollingOffboardingRollupValidator()
    {
        return new RollingOffboardingRollupValidator(
            new List<IRollingOffboardingRollupRule>
            {
                new RollingOffboardingRollupCodeLengthRule(16),
                new RollingOffboardingRollupThresholdRangeRule(4.0, 3025.6),
                new RollingOffboardingRollupRequiredLabelRule("rolling offboarding rollup needs this field"),
                new RollingOffboardingRollupCostCenterLengthRule(86400),
                new RollingOffboardingRollupCodePrefixRule("X"),
            });
    }
}
