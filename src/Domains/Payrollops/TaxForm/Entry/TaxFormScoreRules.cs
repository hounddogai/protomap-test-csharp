namespace Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>A rule a tax form score must pass. Check returns its problems, or none.</summary>
public interface ITaxFormScoreRule
{
    List<string> Check(TaxFormScore item);
}

public sealed record TaxFormScoreRequiredLabelRule(string Message) : ITaxFormScoreRule
{
    public List<string> Check(TaxFormScore item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormScoreWeightRangeRule(double Min, double Max) : ITaxFormScoreRule
{
    public List<string> Check(TaxFormScore item)
    {
        var valueValue = item.Weight;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"weight {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"weight {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormScoreCodeLengthRule(int MaxLength) : ITaxFormScoreRule
{
    public List<string> Check(TaxFormScore item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form score.</summary>
public sealed record TaxFormScoreValidator(List<ITaxFormScoreRule> Rules)
{
    public List<string> Validate(TaxFormScore item)
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

    public void RequireValid(TaxFormScore item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormScore> items)
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

/// <summary>Functions of tax form score rules.</summary>
public static class TaxFormScoreRules
{
    public static TaxFormScoreValidator DefaultTaxFormScoreValidator()
    {
        return new TaxFormScoreValidator(
            new List<ITaxFormScoreRule>
            {
                new TaxFormScoreRequiredLabelRule("tax form score needs an entry"),
                new TaxFormScoreWeightRangeRule(9.2, 754.7),
                new TaxFormScoreCodeLengthRule(16),
            });
    }
}
