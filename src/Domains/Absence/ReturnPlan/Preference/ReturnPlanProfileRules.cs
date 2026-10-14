namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan profile must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanProfileRule
{
    List<string> Check(ReturnPlanProfile item);
}

public sealed record ReturnPlanProfileNameLengthRule(int MaxLength) : IReturnPlanProfileRule
{
    public List<string> Check(ReturnPlanProfile item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProfileAllowedCodeRule(List<string> Allowed) : IReturnPlanProfileRule
{
    public List<string> Check(ReturnPlanProfile item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProfileRequiredNameRule(string Message) : IReturnPlanProfileRule
{
    public List<string> Check(ReturnPlanProfile item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanProfileFloorValueRangeRule(double Min, double Max) : IReturnPlanProfileRule
{
    public List<string> Check(ReturnPlanProfile item)
    {
        var valueValue = item.FloorValue;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"floor value {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"floor value {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanProfileCodeLengthRule(int MaxLength) : IReturnPlanProfileRule
{
    public List<string> Check(ReturnPlanProfile item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan profile.</summary>
public sealed record ReturnPlanProfileValidator(List<IReturnPlanProfileRule> Rules)
{
    public List<string> Validate(ReturnPlanProfile item)
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

    public void RequireValid(ReturnPlanProfile item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanProfile> items)
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

/// <summary>Functions of return plan profile rules.</summary>
public static class ReturnPlanProfileRules
{
    public static ReturnPlanProfileValidator DefaultReturnPlanProfileValidator()
    {
        return new ReturnPlanProfileValidator(
            new List<IReturnPlanProfileRule>
            {
                new ReturnPlanProfileNameLengthRule(80),
                new ReturnPlanProfileAllowedCodeRule(new List<string> { "seasonal" }),
                new ReturnPlanProfileRequiredNameRule("return plan profile needs a value"),
                new ReturnPlanProfileFloorValueRangeRule(8.4, 2048.3),
                new ReturnPlanProfileCodeLengthRule(16),
            });
    }
}
