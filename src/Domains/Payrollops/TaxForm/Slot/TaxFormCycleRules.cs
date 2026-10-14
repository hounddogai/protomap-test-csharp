namespace Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>A rule a tax form cycle must pass. Check returns its problems, or none.</summary>
public interface ITaxFormCycleRule
{
    List<string> Check(TaxFormCycle item);
}

public sealed record TaxFormCycleRequiredCodeRule(string Message) : ITaxFormCycleRule
{
    public List<string> Check(TaxFormCycle item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormCycleRequiredJobCodeRule(string Message) : ITaxFormCycleRule
{
    public List<string> Check(TaxFormCycle item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormCycleCodeLengthRule(int MaxLength) : ITaxFormCycleRule
{
    public List<string> Check(TaxFormCycle item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormCycleStartsAtRangeRule(int Min, int Max) : ITaxFormCycleRule
{
    public List<string> Check(TaxFormCycle item)
    {
        var valueValue = item.StartsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"starts at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form cycle.</summary>
public sealed record TaxFormCycleValidator(List<ITaxFormCycleRule> Rules)
{
    public List<string> Validate(TaxFormCycle item)
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

    public void RequireValid(TaxFormCycle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormCycle> items)
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

/// <summary>Functions of tax form cycle rules.</summary>
public static class TaxFormCycleRules
{
    public static TaxFormCycleValidator DefaultTaxFormCycleValidator()
    {
        return new TaxFormCycleValidator(
            new List<ITaxFormCycleRule>
            {
                new TaxFormCycleRequiredCodeRule("tax form cycle needs an entry"),
                new TaxFormCycleRequiredJobCodeRule("tax form cycle needs a value"),
                new TaxFormCycleCodeLengthRule(64),
                new TaxFormCycleStartsAtRangeRule(64, 1000),
            });
    }
}
