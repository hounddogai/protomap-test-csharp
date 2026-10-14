namespace Acme.Workforce.Payrollops.Garnishment.Exception;

/// <summary>A rule a seasonal garnishment notice must pass. Check returns its problems, or none.</summary>
public interface ISeasonalGarnishmentNoticeRule
{
    List<string> Check(SeasonalGarnishmentNotice item);
}

public sealed record SeasonalGarnishmentNoticeCodePrefixRule(string Prefix) : ISeasonalGarnishmentNoticeRule
{
    public List<string> Check(SeasonalGarnishmentNotice item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentNoticeJobCodeLengthRule(int MaxLength) : ISeasonalGarnishmentNoticeRule
{
    public List<string> Check(SeasonalGarnishmentNotice item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentNoticeEmployeeIdLengthRule(int MaxLength) : ISeasonalGarnishmentNoticeRule
{
    public List<string> Check(SeasonalGarnishmentNotice item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalGarnishmentNoticeMarginRangeRule(double Min, double Max) : ISeasonalGarnishmentNoticeRule
{
    public List<string> Check(SeasonalGarnishmentNotice item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalGarnishmentNoticeRequiredEmployeeIdRule(string Message) : ISeasonalGarnishmentNoticeRule
{
    public List<string> Check(SeasonalGarnishmentNotice item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal garnishment notice.</summary>
public sealed record SeasonalGarnishmentNoticeValidator(List<ISeasonalGarnishmentNoticeRule> Rules)
{
    public List<string> Validate(SeasonalGarnishmentNotice item)
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

    public void RequireValid(SeasonalGarnishmentNotice item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalGarnishmentNotice> items)
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

/// <summary>Functions of seasonal garnishment notice rules.</summary>
public static class SeasonalGarnishmentNoticeRules
{
    public static SeasonalGarnishmentNoticeValidator DefaultSeasonalGarnishmentNoticeValidator()
    {
        return new SeasonalGarnishmentNoticeValidator(
            new List<ISeasonalGarnishmentNoticeRule>
            {
                new SeasonalGarnishmentNoticeCodePrefixRule("REF"),
                new SeasonalGarnishmentNoticeJobCodeLengthRule(1000),
                new SeasonalGarnishmentNoticeEmployeeIdLengthRule(64),
                new SeasonalGarnishmentNoticeMarginRangeRule(5.9, 3048.4),
                new SeasonalGarnishmentNoticeRequiredEmployeeIdRule("seasonal garnishment notice needs a value"),
            });
    }
}
