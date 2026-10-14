namespace Acme.Workforce.Payrollops.TaxForm.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a seasonal tax form threshold must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTaxFormThresholdRule
{
    List<string> Check(SeasonalTaxFormThreshold item);
}

public sealed record SeasonalTaxFormThresholdRequiredDepartmentRule(string Message) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormThresholdMarginRangeRule(double Min, double Max) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
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

public sealed record SeasonalTaxFormThresholdRequiredCodeRule(string Message) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormThresholdCodeLengthRule(int MaxLength) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormThresholdThresholdRangeRule(double Min, double Max) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
    {
        var valueValue = item.Threshold;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"threshold {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"threshold {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record SeasonalTaxFormThresholdAllowedCurrencyRule(List<string> Allowed) : ISeasonalTaxFormThresholdRule
{
    public List<string> Check(SeasonalTaxFormThreshold item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Currency)))
        {
            return new List<string> { $"currency {item.Currency} is not allowed" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal tax form threshold.</summary>
public sealed record SeasonalTaxFormThresholdValidator(List<ISeasonalTaxFormThresholdRule> Rules)
{
    public List<string> Validate(SeasonalTaxFormThreshold item)
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

    public void RequireValid(SeasonalTaxFormThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTaxFormThreshold> items)
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

/// <summary>Functions of seasonal tax form threshold rules.</summary>
public static class SeasonalTaxFormThresholdRules
{
    public static SeasonalTaxFormThresholdValidator DefaultSeasonalTaxFormThresholdValidator()
    {
        return new SeasonalTaxFormThresholdValidator(
            new List<ISeasonalTaxFormThresholdRule>
            {
                new SeasonalTaxFormThresholdRequiredDepartmentRule("seasonal tax form threshold needs an entry"),
                new SeasonalTaxFormThresholdMarginRangeRule(3.1, 2029.1),
                new SeasonalTaxFormThresholdRequiredCodeRule("seasonal tax form threshold needs a value"),
                new SeasonalTaxFormThresholdCodeLengthRule(3600),
                new SeasonalTaxFormThresholdThresholdRangeRule(2.4, 1850.3),
                new SeasonalTaxFormThresholdAllowedCurrencyRule(new List<string> { "lenient" }),
            });
    }
}
