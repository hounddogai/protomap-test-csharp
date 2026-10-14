namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>A rule a seasonal tax form allocation must pass. Check returns its problems, or none.</summary>
public interface ISeasonalTaxFormAllocationRule
{
    List<string> Check(SeasonalTaxFormAllocation item);
}

public sealed record SeasonalTaxFormAllocationRequiredDepartmentRule(string Message) : ISeasonalTaxFormAllocationRule
{
    public List<string> Check(SeasonalTaxFormAllocation item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormAllocationTagsLimitRule(int MaxCount, List<string> Banned) : ISeasonalTaxFormAllocationRule
{
    public List<string> Check(SeasonalTaxFormAllocation item)
    {
        var problems = new List<string>();
        if (item.Tags.Count > MaxCount)
        {
            problems.Add($"has {item.Tags.Count} tags, at most {MaxCount} allowed");
        }
        foreach (var tag in item.Tags)
        {
            if (Banned.Contains(tag))
            {
                problems.Add($"tag {tag} is not allowed");
            }
        }
        return problems;
    }
}

public sealed record SeasonalTaxFormAllocationCapacityRangeRule(int Min, int Max) : ISeasonalTaxFormAllocationRule
{
    public List<string> Check(SeasonalTaxFormAllocation item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormAllocationRequiredNameRule(string Message) : ISeasonalTaxFormAllocationRule
{
    public List<string> Check(SeasonalTaxFormAllocation item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record SeasonalTaxFormAllocationCodeLengthRule(int MaxLength) : ISeasonalTaxFormAllocationRule
{
    public List<string> Check(SeasonalTaxFormAllocation item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a seasonal tax form allocation.</summary>
public sealed record SeasonalTaxFormAllocationValidator(List<ISeasonalTaxFormAllocationRule> Rules)
{
    public List<string> Validate(SeasonalTaxFormAllocation item)
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

    public void RequireValid(SeasonalTaxFormAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<SeasonalTaxFormAllocation> items)
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

/// <summary>Functions of seasonal tax form allocation rules.</summary>
public static class SeasonalTaxFormAllocationRules
{
    public static SeasonalTaxFormAllocationValidator DefaultSeasonalTaxFormAllocationValidator()
    {
        return new SeasonalTaxFormAllocationValidator(
            new List<ISeasonalTaxFormAllocationRule>
            {
                new SeasonalTaxFormAllocationRequiredDepartmentRule("seasonal tax form allocation needs a value"),
                new SeasonalTaxFormAllocationTagsLimitRule(32, new List<string> { "lenient" }),
                new SeasonalTaxFormAllocationCapacityRangeRule(32, 3600),
                new SeasonalTaxFormAllocationRequiredNameRule("seasonal tax form allocation needs a value"),
                new SeasonalTaxFormAllocationCodeLengthRule(3600),
            });
    }
}
