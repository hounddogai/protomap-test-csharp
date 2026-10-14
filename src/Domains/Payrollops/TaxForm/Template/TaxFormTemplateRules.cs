namespace Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>A rule a tax form template must pass. Check returns its problems, or none.</summary>
public interface ITaxFormTemplateRule
{
    List<string> Check(TaxFormTemplate item);
}

public sealed record TaxFormTemplateThresholdRangeRule(double Min, double Max) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
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

public sealed record TaxFormTemplateRequiredDescriptionRule(string Message) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormTemplateVisibleGuardRule(double Limit) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
    {
        if (item.Visible && item.Rating > Limit)
        {
            return new List<string> { $"rating {item.Rating} exceeds {Limit} while visible" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormTemplateRatingRangeRule(double Min, double Max) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
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

public sealed record TaxFormTemplateCodeLengthRule(int MaxLength) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormTemplateLabelLengthRule(int MaxLength) : ITaxFormTemplateRule
{
    public List<string> Check(TaxFormTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form template.</summary>
public sealed record TaxFormTemplateValidator(List<ITaxFormTemplateRule> Rules)
{
    public List<string> Validate(TaxFormTemplate item)
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

    public void RequireValid(TaxFormTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormTemplate> items)
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

/// <summary>Functions of tax form template rules.</summary>
public static class TaxFormTemplateRules
{
    public static TaxFormTemplateValidator DefaultTaxFormTemplateValidator()
    {
        return new TaxFormTemplateValidator(
            new List<ITaxFormTemplateRule>
            {
                new TaxFormTemplateThresholdRangeRule(1.0, 1668.8),
                new TaxFormTemplateRequiredDescriptionRule("tax form template needs this field"),
                new TaxFormTemplateVisibleGuardRule(8.9),
                new TaxFormTemplateRatingRangeRule(8.8, 1863.1),
                new TaxFormTemplateCodeLengthRule(86400),
                new TaxFormTemplateLabelLengthRule(32),
            });
    }
}
