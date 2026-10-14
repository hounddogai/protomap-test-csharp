namespace Acme.Workforce.Talent.Pool.Variance;

/// <summary>A rule a legacy pool variance must pass. Check returns its problems, or none.</summary>
public interface ILegacyPoolVarianceRule
{
    List<string> Check(LegacyPoolVariance item);
}

public sealed record LegacyPoolVarianceRequiredCodeRule(string Message) : ILegacyPoolVarianceRule
{
    public List<string> Check(LegacyPoolVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolVarianceRequiredTimezoneRule(string Message) : ILegacyPoolVarianceRule
{
    public List<string> Check(LegacyPoolVariance item)
    {
        if (item.Timezone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LegacyPoolVarianceCostRangeRule(double Min, double Max) : ILegacyPoolVarianceRule
{
    public List<string> Check(LegacyPoolVariance item)
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

/// <summary>Runs every rule of a legacy pool variance.</summary>
public sealed record LegacyPoolVarianceValidator(List<ILegacyPoolVarianceRule> Rules)
{
    public List<string> Validate(LegacyPoolVariance item)
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

    public void RequireValid(LegacyPoolVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LegacyPoolVariance> items)
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

/// <summary>Functions of legacy pool variance rules.</summary>
public static class LegacyPoolVarianceRules
{
    public static LegacyPoolVarianceValidator DefaultLegacyPoolVarianceValidator()
    {
        return new LegacyPoolVarianceValidator(
            new List<ILegacyPoolVarianceRule>
            {
                new LegacyPoolVarianceRequiredCodeRule("legacy pool variance needs this field"),
                new LegacyPoolVarianceRequiredTimezoneRule("legacy pool variance needs an entry"),
                new LegacyPoolVarianceCostRangeRule(4.4, 2795.2),
            });
    }
}
