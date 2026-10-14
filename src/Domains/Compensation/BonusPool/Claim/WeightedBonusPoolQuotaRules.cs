namespace Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>A rule a weighted bonus pool quota must pass. Check returns its problems, or none.</summary>
public interface IWeightedBonusPoolQuotaRule
{
    List<string> Check(WeightedBonusPoolQuota item);
}

public sealed record WeightedBonusPoolQuotaRatingRangeRule(double Min, double Max) : IWeightedBonusPoolQuotaRule
{
    public List<string> Check(WeightedBonusPoolQuota item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record WeightedBonusPoolQuotaNameLengthRule(int MaxLength) : IWeightedBonusPoolQuotaRule
{
    public List<string> Check(WeightedBonusPoolQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolQuotaTenureMonthsRangeRule(int Min, int Max) : IWeightedBonusPoolQuotaRule
{
    public List<string> Check(WeightedBonusPoolQuota item)
    {
        var valueValue = item.TenureMonths;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"tenure months {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedBonusPoolQuotaRequiredChecksumRule(string Message) : IWeightedBonusPoolQuotaRule
{
    public List<string> Check(WeightedBonusPoolQuota item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted bonus pool quota.</summary>
public sealed record WeightedBonusPoolQuotaValidator(List<IWeightedBonusPoolQuotaRule> Rules)
{
    public List<string> Validate(WeightedBonusPoolQuota item)
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

    public void RequireValid(WeightedBonusPoolQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedBonusPoolQuota> items)
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

/// <summary>Functions of weighted bonus pool quota rules.</summary>
public static class WeightedBonusPoolQuotaRules
{
    public static WeightedBonusPoolQuotaValidator DefaultWeightedBonusPoolQuotaValidator()
    {
        return new WeightedBonusPoolQuotaValidator(
            new List<IWeightedBonusPoolQuotaRule>
            {
                new WeightedBonusPoolQuotaRatingRangeRule(9.6, 123.4),
                new WeightedBonusPoolQuotaNameLengthRule(86400),
                new WeightedBonusPoolQuotaTenureMonthsRangeRule(32, 64),
                new WeightedBonusPoolQuotaRequiredChecksumRule("weighted bonus pool quota needs a value"),
            });
    }
}
