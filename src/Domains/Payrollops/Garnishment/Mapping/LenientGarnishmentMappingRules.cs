namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>A rule a lenient garnishment mapping must pass. Check returns its problems, or none.</summary>
public interface ILenientGarnishmentMappingRule
{
    List<string> Check(LenientGarnishmentMapping item);
}

public sealed record LenientGarnishmentMappingLabelLengthRule(int MaxLength) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentMappingCodeLengthRule(int MaxLength) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentMappingBudgetRangeRule(double Min, double Max) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentMappingDepartmentLengthRule(int MaxLength) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        var size = item.Department.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"department is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentMappingRequiredLabelRule(string Message) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record LenientGarnishmentMappingMaximumRangeRule(double Min, double Max) : ILenientGarnishmentMappingRule
{
    public List<string> Check(LenientGarnishmentMapping item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a lenient garnishment mapping.</summary>
public sealed record LenientGarnishmentMappingValidator(List<ILenientGarnishmentMappingRule> Rules)
{
    public List<string> Validate(LenientGarnishmentMapping item)
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

    public void RequireValid(LenientGarnishmentMapping item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<LenientGarnishmentMapping> items)
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

/// <summary>Functions of lenient garnishment mapping rules.</summary>
public static class LenientGarnishmentMappingRules
{
    public static LenientGarnishmentMappingValidator DefaultLenientGarnishmentMappingValidator()
    {
        return new LenientGarnishmentMappingValidator(
            new List<ILenientGarnishmentMappingRule>
            {
                new LenientGarnishmentMappingLabelLengthRule(3600),
                new LenientGarnishmentMappingCodeLengthRule(32),
                new LenientGarnishmentMappingBudgetRangeRule(5.9, 593.0),
                new LenientGarnishmentMappingDepartmentLengthRule(120),
                new LenientGarnishmentMappingRequiredLabelRule("lenient garnishment mapping needs a value"),
                new LenientGarnishmentMappingMaximumRangeRule(3.6, 2211.6),
            });
    }
}
