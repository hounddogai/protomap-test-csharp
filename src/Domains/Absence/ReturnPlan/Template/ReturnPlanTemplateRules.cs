namespace Acme.Workforce.Absence.ReturnPlan.Template;

/// <summary>A rule a return plan template must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanTemplateRule
{
    List<string> Check(ReturnPlanTemplate item);
}

public sealed record ReturnPlanTemplateAttemptsRangeRule(int Min, int Max) : IReturnPlanTemplateRule
{
    public List<string> Check(ReturnPlanTemplate item)
    {
        var valueValue = item.Attempts;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"attempts {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanTemplateRequiredCodeRule(string Message) : IReturnPlanTemplateRule
{
    public List<string> Check(ReturnPlanTemplate item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanTemplateRequiredChecksumRule(string Message) : IReturnPlanTemplateRule
{
    public List<string> Check(ReturnPlanTemplate item)
    {
        if (item.Checksum.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan template.</summary>
public sealed record ReturnPlanTemplateValidator(List<IReturnPlanTemplateRule> Rules)
{
    public List<string> Validate(ReturnPlanTemplate item)
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

    public void RequireValid(ReturnPlanTemplate item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanTemplate> items)
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

/// <summary>Functions of return plan template rules.</summary>
public static class ReturnPlanTemplateRules
{
    public static ReturnPlanTemplateValidator DefaultReturnPlanTemplateValidator()
    {
        return new ReturnPlanTemplateValidator(
            new List<IReturnPlanTemplateRule>
            {
                new ReturnPlanTemplateAttemptsRangeRule(120, 1000),
                new ReturnPlanTemplateRequiredCodeRule("return plan template needs this field"),
                new ReturnPlanTemplateRequiredChecksumRule("return plan template needs a value"),
            });
    }
}
