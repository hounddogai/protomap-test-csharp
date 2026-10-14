namespace Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>A rule a tax form rollup must pass. Check returns its problems, or none.</summary>
public interface ITaxFormRollupRule
{
    List<string> Check(TaxFormRollup item);
}

public sealed record TaxFormRollupRevisionRangeRule(int Min, int Max) : ITaxFormRollupRule
{
    public List<string> Check(TaxFormRollup item)
    {
        var valueValue = item.Revision;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"revision {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"revision {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record TaxFormRollupGraceDaysRangeRule(int Min, int Max) : ITaxFormRollupRule
{
    public List<string> Check(TaxFormRollup item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRollupCodeLengthRule(int MaxLength) : ITaxFormRollupRule
{
    public List<string> Check(TaxFormRollup item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormRollupCodePrefixRule(string Prefix) : ITaxFormRollupRule
{
    public List<string> Check(TaxFormRollup item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form rollup.</summary>
public sealed record TaxFormRollupValidator(List<ITaxFormRollupRule> Rules)
{
    public List<string> Validate(TaxFormRollup item)
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

    public void RequireValid(TaxFormRollup item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormRollup> items)
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

/// <summary>Functions of tax form rollup rules.</summary>
public static class TaxFormRollupRules
{
    public static TaxFormRollupValidator DefaultTaxFormRollupValidator()
    {
        return new TaxFormRollupValidator(
            new List<ITaxFormRollupRule>
            {
                new TaxFormRollupRevisionRangeRule(32, 120),
                new TaxFormRollupGraceDaysRangeRule(16, 3600),
                new TaxFormRollupCodeLengthRule(3600),
                new TaxFormRollupCodePrefixRule("REF"),
            });
    }
}
