namespace Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>A rule a tax form allocation must pass. Check returns its problems, or none.</summary>
public interface ITaxFormAllocationRule
{
    List<string> Check(TaxFormAllocation item);
}

public sealed record TaxFormAllocationGraceDaysRangeRule(int Min, int Max) : ITaxFormAllocationRule
{
    public List<string> Check(TaxFormAllocation item)
    {
        var valueValue = item.GraceDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"grace days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAllocationRequiredJobCodeRule(string Message) : ITaxFormAllocationRule
{
    public List<string> Check(TaxFormAllocation item)
    {
        if (item.JobCode.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record TaxFormAllocationCodeLengthRule(int MaxLength) : ITaxFormAllocationRule
{
    public List<string> Check(TaxFormAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a tax form allocation.</summary>
public sealed record TaxFormAllocationValidator(List<ITaxFormAllocationRule> Rules)
{
    public List<string> Validate(TaxFormAllocation item)
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

    public void RequireValid(TaxFormAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<TaxFormAllocation> items)
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

/// <summary>Functions of tax form allocation rules.</summary>
public static class TaxFormAllocationRules
{
    public static TaxFormAllocationValidator DefaultTaxFormAllocationValidator()
    {
        return new TaxFormAllocationValidator(
            new List<ITaxFormAllocationRule>
            {
                new TaxFormAllocationGraceDaysRangeRule(64, 80),
                new TaxFormAllocationRequiredJobCodeRule("tax form allocation needs an entry"),
                new TaxFormAllocationCodeLengthRule(255),
            });
    }
}
