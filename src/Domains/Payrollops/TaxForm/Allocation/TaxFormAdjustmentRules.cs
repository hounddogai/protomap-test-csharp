namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a tax form adjustment must pass. Check returns its problems, or none.</summary>
public interface ITaxFormAdjustmentRule
{
    List<string> Check(TaxFormAdjustment item);
}

public sealed record TaxFormAdjustmentCodePrefixRule(string Prefix) : ITaxFormAdjustmentRule
{
    public List<string> Check(TaxFormAdjustment item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAdjustmentAllowedLabelRule(List<string> Allowed) : ITaxFormAdjustmentRule
{
    public List<string> Check(TaxFormAdjustment item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Label)))
        {
            return new List<string> { $"label {item.Label} is not allowed" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAdjustmentLabelLengthRule(int MaxLength) : ITaxFormAdjustmentRule
{
    public List<string> Check(TaxFormAdjustment item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAdjustmentMarginRangeRule(double Min, double Max) : ITaxFormAdjustmentRule
{
    public List<string> Check(TaxFormAdjustment item)
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

/// <summary>Runs every rule of a tax form adjustment.</summary>
public sealed record TaxFormAdjustmentValidator(List<ITaxFormAdjustmentRule> Rules)
{
    public List<string> Validate(TaxFormAdjustment item)
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

    public void RequireValid(TaxFormAdjustment item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormAdjustment> items)
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

/// <summary>Functions of tax form adjustment rules.</summary>
public static class TaxFormAdjustmentRules
{
    public static TaxFormAdjustmentValidator DefaultTaxFormAdjustmentValidator()
    {
        return new TaxFormAdjustmentValidator(
            new List<ITaxFormAdjustmentRule>
            {
                new TaxFormAdjustmentCodePrefixRule("X"),
                new TaxFormAdjustmentAllowedLabelRule(new List<string> { "primary" }),
                new TaxFormAdjustmentLabelLengthRule(86400),
                new TaxFormAdjustmentMarginRangeRule(0.5, 3737.0),
            });
    }
}
