namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

/// <summary>A rule a tax form bundle must pass. Check returns its problems, or none.</summary>
public interface ITaxFormBundleRule
{
    List<string> Check(TaxFormBundle item);
}

public sealed record TaxFormBundleBadgeLengthRule(int MaxLength) : ITaxFormBundleRule
{
    public List<string> Check(TaxFormBundle item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBundleMarginRangeRule(double Min, double Max) : ITaxFormBundleRule
{
    public List<string> Check(TaxFormBundle item)
    {
        var valueValue = item.Margin;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"margin {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBundleRequiredLabelRule(string Message) : ITaxFormBundleRule
{
    public List<string> Check(TaxFormBundle item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form bundle.</summary>
public sealed record TaxFormBundleValidator(List<ITaxFormBundleRule> Rules)
{
    public List<string> Validate(TaxFormBundle item)
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

    public void RequireValid(TaxFormBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormBundle> items)
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

/// <summary>Functions of tax form bundle rules.</summary>
public static class TaxFormBundleRules
{
    public static TaxFormBundleValidator DefaultTaxFormBundleValidator()
    {
        return new TaxFormBundleValidator(
            new List<ITaxFormBundleRule>
            {
                new TaxFormBundleBadgeLengthRule(1000),
                new TaxFormBundleMarginRangeRule(1.0, 400.4),
                new TaxFormBundleRequiredLabelRule("tax form bundle needs a value"),
            });
    }
}
