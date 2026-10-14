namespace Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>A rule a rolling cost model variance must pass. Check returns its problems, or none.</summary>
public interface IRollingCostModelVarianceRule
{
    List<string> Check(RollingCostModelVariance item);
}

public sealed record RollingCostModelVarianceGraceDaysRangeRule(int Min, int Max) : IRollingCostModelVarianceRule
{
    public List<string> Check(RollingCostModelVariance item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelVarianceRatioRangeRule(double Min, double Max) : IRollingCostModelVarianceRule
{
    public List<string> Check(RollingCostModelVariance item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelVariancePinnedGuardRule(double Limit) : IRollingCostModelVarianceRule
{
    public List<string> Check(RollingCostModelVariance item)
    {
        if (item.Pinned && item.Threshold > Limit)
        {
            return new List<string> { $"threshold {item.Threshold} exceeds {Limit} while pinned" };
        }
        return new List<string>();
    }
}

public sealed record RollingCostModelVarianceRequiredCodeRule(string Message) : IRollingCostModelVarianceRule
{
    public List<string> Check(RollingCostModelVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling cost model variance.</summary>
public sealed record RollingCostModelVarianceValidator(List<IRollingCostModelVarianceRule> Rules)
{
    public List<string> Validate(RollingCostModelVariance item)
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

    public void RequireValid(RollingCostModelVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingCostModelVariance> items)
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

/// <summary>Functions of rolling cost model variance rules.</summary>
public static class RollingCostModelVarianceRules
{
    public static RollingCostModelVarianceValidator DefaultRollingCostModelVarianceValidator()
    {
        return new RollingCostModelVarianceValidator(
            new List<IRollingCostModelVarianceRule>
            {
                new RollingCostModelVarianceGraceDaysRangeRule(80, 80),
                new RollingCostModelVarianceRatioRangeRule(1.9, 174.4),
                new RollingCostModelVariancePinnedGuardRule(5.0),
                new RollingCostModelVarianceRequiredCodeRule("rolling cost model variance needs a value"),
            });
    }
}
