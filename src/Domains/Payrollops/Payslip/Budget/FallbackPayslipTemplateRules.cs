namespace Acme.Workforce.Payrollops.Payslip.Budget;

/// <summary>A rule a fallback payslip template must pass. Check returns its problems, or none.</summary>
public interface IFallbackPayslipTemplateRule
{
    List<string> Check(FallbackPayslipTemplate item);
}

public sealed record FallbackPayslipTemplateNameLengthRule(int MaxLength) : IFallbackPayslipTemplateRule
{
    public List<string> Check(FallbackPayslipTemplate item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipTemplateRequiredNameRule(string Message) : IFallbackPayslipTemplateRule
{
    public List<string> Check(FallbackPayslipTemplate item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipTemplateUnitLengthRule(int MaxLength) : IFallbackPayslipTemplateRule
{
    public List<string> Check(FallbackPayslipTemplate item)
    {
        var size = item.Unit.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"unit is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record FallbackPayslipTemplateFloorValueRangeRule(double Min, double Max) : IFallbackPayslipTemplateRule
{
    public List<string> Check(FallbackPayslipTemplate item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a fallback payslip template.</summary>
public sealed record FallbackPayslipTemplateValidator(List<IFallbackPayslipTemplateRule> Rules)
{
    public List<string> Validate(FallbackPayslipTemplate item)
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

    public void RequireValid(FallbackPayslipTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<FallbackPayslipTemplate> items)
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

/// <summary>Functions of fallback payslip template rules.</summary>
public static class FallbackPayslipTemplateRules
{
    public static FallbackPayslipTemplateValidator DefaultFallbackPayslipTemplateValidator()
    {
        return new FallbackPayslipTemplateValidator(
            new List<IFallbackPayslipTemplateRule>
            {
                new FallbackPayslipTemplateNameLengthRule(32),
                new FallbackPayslipTemplateRequiredNameRule("fallback payslip template needs this field"),
                new FallbackPayslipTemplateUnitLengthRule(86400),
                new FallbackPayslipTemplateFloorValueRangeRule(4.7, 466.5),
            });
    }
}
