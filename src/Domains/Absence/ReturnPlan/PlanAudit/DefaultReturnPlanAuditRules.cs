namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>A rule a default return plan audit must pass. Check returns its problems, or none.</summary>
public interface IDefaultReturnPlanAuditRule
{
    List<string> Check(DefaultReturnPlanAudit item);
}

public sealed record DefaultReturnPlanAuditRequiredCategoryRule(string Message) : IDefaultReturnPlanAuditRule
{
    public List<string> Check(DefaultReturnPlanAudit item)
    {
        if (item.Category.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanAuditRequiredPhoneRule(string Message) : IDefaultReturnPlanAuditRule
{
    public List<string> Check(DefaultReturnPlanAudit item)
    {
        if (item.Phone.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanAuditWindowDaysRangeRule(int Min, int Max) : IDefaultReturnPlanAuditRule
{
    public List<string> Check(DefaultReturnPlanAudit item)
    {
        var valueValue = item.WindowDays;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"window days {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanAuditTitleLengthRule(int MaxLength) : IDefaultReturnPlanAuditRule
{
    public List<string> Check(DefaultReturnPlanAudit item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record DefaultReturnPlanAuditCodeLengthRule(int MaxLength) : IDefaultReturnPlanAuditRule
{
    public List<string> Check(DefaultReturnPlanAudit item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a default return plan audit.</summary>
public sealed record DefaultReturnPlanAuditValidator(List<IDefaultReturnPlanAuditRule> Rules)
{
    public List<string> Validate(DefaultReturnPlanAudit item)
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

    public void RequireValid(DefaultReturnPlanAudit item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<DefaultReturnPlanAudit> items)
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

/// <summary>Functions of default return plan audit rules.</summary>
public static class DefaultReturnPlanAuditRules
{
    public static DefaultReturnPlanAuditValidator DefaultDefaultReturnPlanAuditValidator()
    {
        return new DefaultReturnPlanAuditValidator(
            new List<IDefaultReturnPlanAuditRule>
            {
                new DefaultReturnPlanAuditRequiredCategoryRule("default return plan audit needs an entry"),
                new DefaultReturnPlanAuditRequiredPhoneRule("default return plan audit needs this field"),
                new DefaultReturnPlanAuditWindowDaysRangeRule(32, 255),
                new DefaultReturnPlanAuditTitleLengthRule(64),
                new DefaultReturnPlanAuditCodeLengthRule(64),
            });
    }
}
