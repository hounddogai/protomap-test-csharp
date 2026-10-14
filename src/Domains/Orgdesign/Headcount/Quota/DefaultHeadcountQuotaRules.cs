namespace Acme.Workforce.Orgdesign.Headcount.Quota;

/// <summary>A rule a default headcount quota must pass. Check returns its problems, or none.</summary>
public interface IDefaultHeadcountQuotaRule
{
    List<string> Check(DefaultHeadcountQuota item);
}

public sealed record DefaultHeadcountQuotaGraceDaysRangeRule(int Min, int Max) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountQuotaRequiredNameRule(string Message) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountQuotaRatioRangeRule(double Min, double Max) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        var valueValue = item.Ratio;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"ratio {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"ratio {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DefaultHeadcountQuotaCountryLengthRule(int MaxLength) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        var size = item.Country.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"country is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountQuotaCodeLengthRule(int MaxLength) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultHeadcountQuotaNameLengthRule(int MaxLength) : IDefaultHeadcountQuotaRule
{
    public List<string> Check(DefaultHeadcountQuota item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default headcount quota.</summary>
public sealed record DefaultHeadcountQuotaValidator(List<IDefaultHeadcountQuotaRule> Rules)
{
    public List<string> Validate(DefaultHeadcountQuota item)
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

    public void RequireValid(DefaultHeadcountQuota item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultHeadcountQuota> items)
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

/// <summary>Functions of default headcount quota rules.</summary>
public static class DefaultHeadcountQuotaRules
{
    public static DefaultHeadcountQuotaValidator DefaultDefaultHeadcountQuotaValidator()
    {
        return new DefaultHeadcountQuotaValidator(
            new List<IDefaultHeadcountQuotaRule>
            {
                new DefaultHeadcountQuotaGraceDaysRangeRule(64, 86400),
                new DefaultHeadcountQuotaRequiredNameRule("default headcount quota needs an entry"),
                new DefaultHeadcountQuotaRatioRangeRule(1.3, 4439.9),
                new DefaultHeadcountQuotaCountryLengthRule(16),
                new DefaultHeadcountQuotaCodeLengthRule(1000),
                new DefaultHeadcountQuotaNameLengthRule(3600),
            });
    }
}
