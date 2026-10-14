namespace Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>A rule a return plan window must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanWindowRule
{
    List<string> Check(ReturnPlanWindow item);
}

public sealed record ReturnPlanWindowRequiredCostCenterRule(string Message) : IReturnPlanWindowRule
{
    public List<string> Check(ReturnPlanWindow item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanWindowRequiredOwnerIdRule(string Message) : IReturnPlanWindowRule
{
    public List<string> Check(ReturnPlanWindow item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanWindowVersionRangeRule(int Min, int Max) : IReturnPlanWindowRule
{
    public List<string> Check(ReturnPlanWindow item)
    {
        var valueValue = item.Version;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"version {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanWindowCodeLengthRule(int MaxLength) : IReturnPlanWindowRule
{
    public List<string> Check(ReturnPlanWindow item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan window.</summary>
public sealed record ReturnPlanWindowValidator(List<IReturnPlanWindowRule> Rules)
{
    public List<string> Validate(ReturnPlanWindow item)
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

    public void RequireValid(ReturnPlanWindow item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanWindow> items)
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

/// <summary>Functions of return plan window rules.</summary>
public static class ReturnPlanWindowRules
{
    public static ReturnPlanWindowValidator DefaultReturnPlanWindowValidator()
    {
        return new ReturnPlanWindowValidator(
            new List<IReturnPlanWindowRule>
            {
                new ReturnPlanWindowRequiredCostCenterRule("return plan window needs this field"),
                new ReturnPlanWindowRequiredOwnerIdRule("return plan window needs this field"),
                new ReturnPlanWindowVersionRangeRule(120, 3600),
                new ReturnPlanWindowCodeLengthRule(80),
            });
    }
}
