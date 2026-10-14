namespace Acme.Workforce.Payrollops.Deduction.Proposal;

/// <summary>A rule a deduction template must pass. Check returns its problems, or none.</summary>
public interface IDeductionTemplateRule
{
    List<string> Check(DeductionTemplate item);
}

public sealed record DeductionTemplateCostRangeRule(double Min, double Max) : IDeductionTemplateRule
{
    public List<string> Check(DeductionTemplate item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record DeductionTemplateThresholdRangeRule(double Min, double Max) : IDeductionTemplateRule
{
    public List<string> Check(DeductionTemplate item)
    {
        var valueValue = item.Threshold;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"threshold {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionTemplateLabelLengthRule(int MaxLength) : IDeductionTemplateRule
{
    public List<string> Check(DeductionTemplate item)
    {
        var size = item.Label.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"label is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DeductionTemplateRequiredCodeRule(string Message) : IDeductionTemplateRule
{
    public List<string> Check(DeductionTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DeductionTemplateCodeLengthRule(int MaxLength) : IDeductionTemplateRule
{
    public List<string> Check(DeductionTemplate item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a deduction template.</summary>
public sealed record DeductionTemplateValidator(List<IDeductionTemplateRule> Rules)
{
    public List<string> Validate(DeductionTemplate item)
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

    public void RequireValid(DeductionTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DeductionTemplate> items)
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

/// <summary>Functions of deduction template rules.</summary>
public static class DeductionTemplateRules
{
    public static DeductionTemplateValidator DefaultDeductionTemplateValidator()
    {
        return new DeductionTemplateValidator(
            new List<IDeductionTemplateRule>
            {
                new DeductionTemplateCostRangeRule(7.0, 182.7),
                new DeductionTemplateThresholdRangeRule(8.6, 2227.2),
                new DeductionTemplateLabelLengthRule(32),
                new DeductionTemplateRequiredCodeRule("deduction template needs an entry"),
                new DeductionTemplateCodeLengthRule(3600),
            });
    }
}
