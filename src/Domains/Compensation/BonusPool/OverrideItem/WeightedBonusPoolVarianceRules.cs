namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>A rule a weighted bonus pool variance must pass. Check returns its problems, or none.</summary>
public interface IWeightedBonusPoolVarianceRule
{
    List<string> Check(WeightedBonusPoolVariance item);
}

public sealed record WeightedBonusPoolVarianceRequiredChecksumRule(string Message) : IWeightedBonusPoolVarianceRule
{
    public List<string> Check(WeightedBonusPoolVariance item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolVarianceHoursRangeRule(double Min, double Max) : IWeightedBonusPoolVarianceRule
{
    public List<string> Check(WeightedBonusPoolVariance item)
    {
        var valueValue = item.Hours;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"hours {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"hours {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedBonusPoolVarianceMinimumRangeRule(double Min, double Max) : IWeightedBonusPoolVarianceRule
{
    public List<string> Check(WeightedBonusPoolVariance item)
    {
        var valueValue = item.Minimum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"minimum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted bonus pool variance.</summary>
public sealed record WeightedBonusPoolVarianceValidator(List<IWeightedBonusPoolVarianceRule> Rules)
{
    public List<string> Validate(WeightedBonusPoolVariance item)
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

    public void RequireValid(WeightedBonusPoolVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBonusPoolVariance> items)
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

/// <summary>Functions of weighted bonus pool variance rules.</summary>
public static class WeightedBonusPoolVarianceRules
{
    public static WeightedBonusPoolVarianceValidator DefaultWeightedBonusPoolVarianceValidator()
    {
        return new WeightedBonusPoolVarianceValidator(
            new List<IWeightedBonusPoolVarianceRule>
            {
                new WeightedBonusPoolVarianceRequiredChecksumRule("weighted bonus pool variance needs an entry"),
                new WeightedBonusPoolVarianceHoursRangeRule(1.0, 3816.4),
                new WeightedBonusPoolVarianceMinimumRangeRule(6.8, 2908.5),
            });
    }
}
