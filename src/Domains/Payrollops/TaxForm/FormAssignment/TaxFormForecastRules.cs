namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form forecast must pass. Check returns its problems, or none.</summary>
public interface ITaxFormForecastRule
{
    List<string> Check(TaxFormForecast item);
}

public sealed record TaxFormForecastTitleLengthRule(int MaxLength) : ITaxFormForecastRule
{
    public List<string> Check(TaxFormForecast item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormForecastEndsAtRangeRule(int Min, int Max) : ITaxFormForecastRule
{
    public List<string> Check(TaxFormForecast item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormForecastAllowedCodeRule(List<string> Allowed) : ITaxFormForecastRule
{
    public List<string> Check(TaxFormForecast item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is expired" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormForecastRequiredTitleRule(string Message) : ITaxFormForecastRule
{
    public List<string> Check(TaxFormForecast item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form forecast.</summary>
public sealed record TaxFormForecastValidator(List<ITaxFormForecastRule> Rules)
{
    public List<string> Validate(TaxFormForecast item)
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

    public void RequireValid(TaxFormForecast item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormForecast> items)
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

/// <summary>Functions of tax form forecast rules.</summary>
public static class TaxFormForecastRules
{
    public static TaxFormForecastValidator DefaultTaxFormForecastValidator()
    {
        return new TaxFormForecastValidator(
            new List<ITaxFormForecastRule>
            {
                new TaxFormForecastTitleLengthRule(1000),
                new TaxFormForecastEndsAtRangeRule(16, 64),
                new TaxFormForecastAllowedCodeRule(new List<string> { "weighted" }),
                new TaxFormForecastRequiredTitleRule("tax form forecast needs a value"),
            });
    }
}
