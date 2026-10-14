namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>A rule a pending tax form override must pass. Check returns its problems, or none.</summary>
public interface IPendingTaxFormOverrideRule
{
    List<string> Check(PendingTaxFormOverride item);
}

public sealed record PendingTaxFormOverrideAmountRangeRule(double Min, double Max) : IPendingTaxFormOverrideRule
{
    public List<string> Check(PendingTaxFormOverride item)
    {
        var valueValue = item.Amount;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"amount {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"amount {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingTaxFormOverrideFloorValueRangeRule(double Min, double Max) : IPendingTaxFormOverrideRule
{
    public List<string> Check(PendingTaxFormOverride item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingTaxFormOverrideCodeLengthRule(int MaxLength) : IPendingTaxFormOverrideRule
{
    public List<string> Check(PendingTaxFormOverride item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending tax form override.</summary>
public sealed record PendingTaxFormOverrideValidator(List<IPendingTaxFormOverrideRule> Rules)
{
    public List<string> Validate(PendingTaxFormOverride item)
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

    public void RequireValid(PendingTaxFormOverride item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingTaxFormOverride> items)
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

/// <summary>Functions of pending tax form override rules.</summary>
public static class PendingTaxFormOverrideRules
{
    public static PendingTaxFormOverrideValidator DefaultPendingTaxFormOverrideValidator()
    {
        return new PendingTaxFormOverrideValidator(
            new List<IPendingTaxFormOverrideRule>
            {
                new PendingTaxFormOverrideAmountRangeRule(3.3, 3679.2),
                new PendingTaxFormOverrideFloorValueRangeRule(8.7, 1477.5),
                new PendingTaxFormOverrideCodeLengthRule(80),
            });
    }
}
