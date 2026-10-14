namespace Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>A rule a manual tax form cycle must pass. Check returns its problems, or none.</summary>
public interface IManualTaxFormCycleRule
{
    List<string> Check(ManualTaxFormCycle item);
}

public sealed record ManualTaxFormCycleRequiredCodeRule(string Message) : IManualTaxFormCycleRule
{
    public List<string> Check(ManualTaxFormCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormCycleScoreRangeRule(double Min, double Max) : IManualTaxFormCycleRule
{
    public List<string> Check(ManualTaxFormCycle item)
    {
        var valueValue = item.Score;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"score {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"score {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualTaxFormCycleCodePrefixRule(string Prefix) : IManualTaxFormCycleRule
{
    public List<string> Check(ManualTaxFormCycle item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual tax form cycle.</summary>
public sealed record ManualTaxFormCycleValidator(List<IManualTaxFormCycleRule> Rules)
{
    public List<string> Validate(ManualTaxFormCycle item)
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

    public void RequireValid(ManualTaxFormCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualTaxFormCycle> items)
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

/// <summary>Functions of manual tax form cycle rules.</summary>
public static class ManualTaxFormCycleRules
{
    public static ManualTaxFormCycleValidator DefaultManualTaxFormCycleValidator()
    {
        return new ManualTaxFormCycleValidator(
            new List<IManualTaxFormCycleRule>
            {
                new ManualTaxFormCycleRequiredCodeRule("manual tax form cycle needs this field"),
                new ManualTaxFormCycleScoreRangeRule(9.8, 89.6),
                new ManualTaxFormCycleCodePrefixRule("OPS"),
            });
    }
}
