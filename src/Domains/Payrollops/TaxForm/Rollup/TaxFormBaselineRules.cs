namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>A rule a tax form baseline must pass. Check returns its problems, or none.</summary>
public interface ITaxFormBaselineRule
{
    List<string> Check(TaxFormBaseline item);
}

public sealed record TaxFormBaselinePriorityRangeRule(int Min, int Max) : ITaxFormBaselineRule
{
    public List<string> Check(TaxFormBaseline item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBaselineRequiredCodeRule(string Message) : ITaxFormBaselineRule
{
    public List<string> Check(TaxFormBaseline item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormBaselineQuantityRangeRule(int Min, int Max) : ITaxFormBaselineRule
{
    public List<string> Check(TaxFormBaseline item)
    {
        var valueValue = item.Quantity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"quantity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"quantity {valueValue} is above {Max}");
        }
        return problems;
    }
}

/// <summary>Runs every rule of a tax form baseline.</summary>
public sealed record TaxFormBaselineValidator(List<ITaxFormBaselineRule> Rules)
{
    public List<string> Validate(TaxFormBaseline item)
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

    public void RequireValid(TaxFormBaseline item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormBaseline> items)
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

/// <summary>Functions of tax form baseline rules.</summary>
public static class TaxFormBaselineRules
{
    public static TaxFormBaselineValidator DefaultTaxFormBaselineValidator()
    {
        return new TaxFormBaselineValidator(
            new List<ITaxFormBaselineRule>
            {
                new TaxFormBaselinePriorityRangeRule(1000, 3600),
                new TaxFormBaselineRequiredCodeRule("tax form baseline needs this field"),
                new TaxFormBaselineQuantityRangeRule(64, 1000),
            });
    }
}
