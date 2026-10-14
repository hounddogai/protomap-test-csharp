namespace Acme.Workforce.Absence.ReturnPlan.Preference;

/// <summary>A rule a return plan preference must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanPreferenceRule
{
    List<string> Check(ReturnPlanPreference item);
}

public sealed record ReturnPlanPreferenceRequiredDepartmentRule(string Message) : IReturnPlanPreferenceRule
{
    public List<string> Check(ReturnPlanPreference item)
    {
        if (item.Department.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPreferenceQuantityRangeRule(int Min, int Max) : IReturnPlanPreferenceRule
{
    public List<string> Check(ReturnPlanPreference item)
    {
        var valueValue = item.Quantity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"quantity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPreferenceTitleLengthRule(int MaxLength) : IReturnPlanPreferenceRule
{
    public List<string> Check(ReturnPlanPreference item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanPreferenceRatioRangeRule(double Min, double Max) : IReturnPlanPreferenceRule
{
    public List<string> Check(ReturnPlanPreference item)
    {
        var valueValue = item.Ratio;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ratio {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan preference.</summary>
public sealed record ReturnPlanPreferenceValidator(List<IReturnPlanPreferenceRule> Rules)
{
    public List<string> Validate(ReturnPlanPreference item)
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

    public void RequireValid(ReturnPlanPreference item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanPreference> items)
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

/// <summary>Functions of return plan preference rules.</summary>
public static class ReturnPlanPreferenceRules
{
    public static ReturnPlanPreferenceValidator DefaultReturnPlanPreferenceValidator()
    {
        return new ReturnPlanPreferenceValidator(
            new List<IReturnPlanPreferenceRule>
            {
                new ReturnPlanPreferenceRequiredDepartmentRule("return plan preference needs a value"),
                new ReturnPlanPreferenceQuantityRangeRule(80, 120),
                new ReturnPlanPreferenceTitleLengthRule(1000),
                new ReturnPlanPreferenceRatioRangeRule(3.2, 344.0),
            });
    }
}
