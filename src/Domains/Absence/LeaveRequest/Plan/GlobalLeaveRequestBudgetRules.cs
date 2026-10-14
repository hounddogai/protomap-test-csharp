namespace Acme.Workforce.Absence.LeaveRequest.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a global leave request budget must pass. Check returns its problems, or none.</summary>
public interface IGlobalLeaveRequestBudgetRule
{
    List<string> Check(GlobalLeaveRequestBudget item);
}

public sealed record GlobalLeaveRequestBudgetGradeLengthRule(int MaxLength) : IGlobalLeaveRequestBudgetRule
{
    public List<string> Check(GlobalLeaveRequestBudget item)
    {
        var size = item.Grade.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"grade is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLeaveRequestBudgetExternalRefLengthRule(int MaxLength) : IGlobalLeaveRequestBudgetRule
{
    public List<string> Check(GlobalLeaveRequestBudget item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLeaveRequestBudgetCodeLengthRule(int MaxLength) : IGlobalLeaveRequestBudgetRule
{
    public List<string> Check(GlobalLeaveRequestBudget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record GlobalLeaveRequestBudgetAllowedGradeRule(List<string> Allowed) : IGlobalLeaveRequestBudgetRule
{
    public List<string> Check(GlobalLeaveRequestBudget item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Grade)))
        {
            return new List<string> { $"grade {item.Grade} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a global leave request budget.</summary>
public sealed record GlobalLeaveRequestBudgetValidator(List<IGlobalLeaveRequestBudgetRule> Rules)
{
    public List<string> Validate(GlobalLeaveRequestBudget item)
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

    public void RequireValid(GlobalLeaveRequestBudget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<GlobalLeaveRequestBudget> items)
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

/// <summary>Functions of global leave request budget rules.</summary>
public static class GlobalLeaveRequestBudgetRules
{
    public static GlobalLeaveRequestBudgetValidator DefaultGlobalLeaveRequestBudgetValidator()
    {
        return new GlobalLeaveRequestBudgetValidator(
            new List<IGlobalLeaveRequestBudgetRule>
            {
                new GlobalLeaveRequestBudgetGradeLengthRule(32),
                new GlobalLeaveRequestBudgetExternalRefLengthRule(255),
                new GlobalLeaveRequestBudgetCodeLengthRule(120),
                new GlobalLeaveRequestBudgetAllowedGradeRule(new List<string> { "legacy", "seasonal" }),
            });
    }
}
