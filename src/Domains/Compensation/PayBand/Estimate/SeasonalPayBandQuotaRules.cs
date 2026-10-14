namespace Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>A rule a seasonal pay band quota must pass. Check returns its problems, or none.</summary>
public interface ISeasonalPayBandQuotaRule
{
    List<string> Check(SeasonalPayBandQuota item);
}

public sealed record SeasonalPayBandQuotaRequiredJobCodeRule(string Message) : ISeasonalPayBandQuotaRule
{
    public List<string> Check(SeasonalPayBandQuota item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalPayBandQuotaSequenceRangeRule(int Min, int Max) : ISeasonalPayBandQuotaRule
{
    public List<string> Check(SeasonalPayBandQuota item)
    {
        var valueValue = item.Sequence;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"sequence {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"sequence {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalPayBandQuotaCodeLengthRule(int MaxLength) : ISeasonalPayBandQuotaRule
{
    public List<string> Check(SeasonalPayBandQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal pay band quota.</summary>
public sealed record SeasonalPayBandQuotaValidator(List<ISeasonalPayBandQuotaRule> Rules)
{
    public List<string> Validate(SeasonalPayBandQuota item)
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

    public void RequireValid(SeasonalPayBandQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalPayBandQuota> items)
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

/// <summary>Functions of seasonal pay band quota rules.</summary>
public static class SeasonalPayBandQuotaRules
{
    public static SeasonalPayBandQuotaValidator DefaultSeasonalPayBandQuotaValidator()
    {
        return new SeasonalPayBandQuotaValidator(
            new List<ISeasonalPayBandQuotaRule>
            {
                new SeasonalPayBandQuotaRequiredJobCodeRule("seasonal pay band quota needs an entry"),
                new SeasonalPayBandQuotaSequenceRangeRule(64, 80),
                new SeasonalPayBandQuotaCodeLengthRule(120),
            });
    }
}
