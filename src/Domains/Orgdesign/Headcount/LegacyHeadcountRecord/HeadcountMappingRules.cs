namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>A rule a headcount mapping must pass. Check returns its problems, or none.</summary>
public interface IHeadcountMappingRule
{
    List<string> Check(HeadcountMapping item);
}

public sealed record HeadcountMappingNameLengthRule(int MaxLength) : IHeadcountMappingRule
{
    public List<string> Check(HeadcountMapping item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountMappingCostCenterLengthRule(int MaxLength) : IHeadcountMappingRule
{
    public List<string> Check(HeadcountMapping item)
    {
        var size = item.CostCenter.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"cost center is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountMappingLabelLengthRule(int MaxLength) : IHeadcountMappingRule
{
    public List<string> Check(HeadcountMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record HeadcountMappingRequiredNameRule(string Message) : IHeadcountMappingRule
{
    public List<string> Check(HeadcountMapping item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a headcount mapping.</summary>
public sealed record HeadcountMappingValidator(List<IHeadcountMappingRule> Rules)
{
    public List<string> Validate(HeadcountMapping item)
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

    public void RequireValid(HeadcountMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<HeadcountMapping> items)
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

/// <summary>Functions of headcount mapping rules.</summary>
public static class HeadcountMappingRules
{
    public static HeadcountMappingValidator DefaultHeadcountMappingValidator()
    {
        return new HeadcountMappingValidator(
            new List<IHeadcountMappingRule>
            {
                new HeadcountMappingNameLengthRule(64),
                new HeadcountMappingCostCenterLengthRule(64),
                new HeadcountMappingLabelLengthRule(32),
                new HeadcountMappingRequiredNameRule("headcount mapping needs a value"),
            });
    }
}
