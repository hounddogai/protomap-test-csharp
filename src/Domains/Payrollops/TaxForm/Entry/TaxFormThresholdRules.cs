namespace Acme.Workforce.Payrollops.TaxForm.Entry;

/// <summary>A rule a tax form threshold must pass. Check returns its problems, or none.</summary>
public interface ITaxFormThresholdRule
{
    List<string> Check(TaxFormThreshold item);
}

public sealed record TaxFormThresholdQuantityRangeRule(int Min, int Max) : ITaxFormThresholdRule
{
    public List<string> Check(TaxFormThreshold item)
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

public sealed record TaxFormThresholdSequenceRangeRule(int Min, int Max) : ITaxFormThresholdRule
{
    public List<string> Check(TaxFormThreshold item)
    {
        var valueValue = item.Sequence;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"sequence {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormThresholdCodePrefixRule(string Prefix) : ITaxFormThresholdRule
{
    public List<string> Check(TaxFormThreshold item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormThresholdRequiredNotesRule(string Message) : ITaxFormThresholdRule
{
    public List<string> Check(TaxFormThreshold item)
    {
        if (item.Notes.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormThresholdCodeLengthRule(int MaxLength) : ITaxFormThresholdRule
{
    public List<string> Check(TaxFormThreshold item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form threshold.</summary>
public sealed record TaxFormThresholdValidator(List<ITaxFormThresholdRule> Rules)
{
    public List<string> Validate(TaxFormThreshold item)
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

    public void RequireValid(TaxFormThreshold item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormThreshold> items)
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

/// <summary>Functions of tax form threshold rules.</summary>
public static class TaxFormThresholdRules
{
    public static TaxFormThresholdValidator DefaultTaxFormThresholdValidator()
    {
        return new TaxFormThresholdValidator(
            new List<ITaxFormThresholdRule>
            {
                new TaxFormThresholdQuantityRangeRule(16, 1000),
                new TaxFormThresholdSequenceRangeRule(120, 1000),
                new TaxFormThresholdCodePrefixRule("OPS"),
                new TaxFormThresholdRequiredNotesRule("tax form threshold needs this field"),
                new TaxFormThresholdCodeLengthRule(16),
            });
    }
}
