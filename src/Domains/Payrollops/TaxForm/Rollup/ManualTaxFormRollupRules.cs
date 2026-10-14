namespace Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>A rule a manual tax form rollup must pass. Check returns its problems, or none.</summary>
public interface IManualTaxFormRollupRule
{
    List<string> Check(ManualTaxFormRollup item);
}

public sealed record ManualTaxFormRollupCodePrefixRule(string Prefix) : IManualTaxFormRollupRule
{
    public List<string> Check(ManualTaxFormRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormRollupRequiredCodeRule(string Message) : IManualTaxFormRollupRule
{
    public List<string> Check(ManualTaxFormRollup item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormRollupMaximumRangeRule(double Min, double Max) : IManualTaxFormRollupRule
{
    public List<string> Check(ManualTaxFormRollup item)
    {
        var valueValue = item.Maximum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"maximum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"maximum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ManualTaxFormRollupRequiredSalaryBandRule(string Message) : IManualTaxFormRollupRule
{
    public List<string> Check(ManualTaxFormRollup item)
    {
        if (item.SalaryBand.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ManualTaxFormRollupSalaryBandLengthRule(int MaxLength) : IManualTaxFormRollupRule
{
    public List<string> Check(ManualTaxFormRollup item)
    {
        var size = item.SalaryBand.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"salary band is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a manual tax form rollup.</summary>
public sealed record ManualTaxFormRollupValidator(List<IManualTaxFormRollupRule> Rules)
{
    public List<string> Validate(ManualTaxFormRollup item)
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

    public void RequireValid(ManualTaxFormRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ManualTaxFormRollup> items)
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

/// <summary>Functions of manual tax form rollup rules.</summary>
public static class ManualTaxFormRollupRules
{
    public static ManualTaxFormRollupValidator DefaultManualTaxFormRollupValidator()
    {
        return new ManualTaxFormRollupValidator(
            new List<IManualTaxFormRollupRule>
            {
                new ManualTaxFormRollupCodePrefixRule("OPS"),
                new ManualTaxFormRollupRequiredCodeRule("manual tax form rollup needs a value"),
                new ManualTaxFormRollupMaximumRangeRule(8.0, 692.2),
                new ManualTaxFormRollupRequiredSalaryBandRule("manual tax form rollup needs a value"),
                new ManualTaxFormRollupSalaryBandLengthRule(255),
            });
    }
}
