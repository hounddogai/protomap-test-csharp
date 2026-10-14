namespace Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>A rule a seasonal deduction quota must pass. Check returns its problems, or none.</summary>
public interface ISeasonalDeductionQuotaRule
{
    List<string> Check(SeasonalDeductionQuota item);
}

public sealed record SeasonalDeductionQuotaDiscountRangeRule(double Min, double Max) : ISeasonalDeductionQuotaRule
{
    public List<string> Check(SeasonalDeductionQuota item)
    {
        var valueValue = item.Discount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"discount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"discount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalDeductionQuotaRequiredLabelRule(string Message) : ISeasonalDeductionQuotaRule
{
    public List<string> Check(SeasonalDeductionQuota item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionQuotaRateRangeRule(double Min, double Max) : ISeasonalDeductionQuotaRule
{
    public List<string> Check(SeasonalDeductionQuota item)
    {
        var valueValue = item.Rate;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rate {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rate {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalDeductionQuotaTitleLengthRule(int MaxLength) : ISeasonalDeductionQuotaRule
{
    public List<string> Check(SeasonalDeductionQuota item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalDeductionQuotaLabelLengthRule(int MaxLength) : ISeasonalDeductionQuotaRule
{
    public List<string> Check(SeasonalDeductionQuota item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal deduction quota.</summary>
public sealed record SeasonalDeductionQuotaValidator(List<ISeasonalDeductionQuotaRule> Rules)
{
    public List<string> Validate(SeasonalDeductionQuota item)
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

    public void RequireValid(SeasonalDeductionQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalDeductionQuota> items)
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

/// <summary>Functions of seasonal deduction quota rules.</summary>
public static class SeasonalDeductionQuotaRules
{
    public static SeasonalDeductionQuotaValidator DefaultSeasonalDeductionQuotaValidator()
    {
        return new SeasonalDeductionQuotaValidator(
            new List<ISeasonalDeductionQuotaRule>
            {
                new SeasonalDeductionQuotaDiscountRangeRule(4.1, 3150.8),
                new SeasonalDeductionQuotaRequiredLabelRule("seasonal deduction quota needs an entry"),
                new SeasonalDeductionQuotaRateRangeRule(1.2, 2395.3),
                new SeasonalDeductionQuotaTitleLengthRule(255),
                new SeasonalDeductionQuotaLabelLengthRule(1000),
            });
    }
}
