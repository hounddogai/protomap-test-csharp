namespace Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>A rule a retro pay summary must pass. Check returns its problems, or none.</summary>
public interface IRetroPaySummaryRule
{
    List<string> Check(RetroPaySummary item);
}

public sealed record RetroPaySummaryBadgeLengthRule(int MaxLength) : IRetroPaySummaryRule
{
    public List<string> Check(RetroPaySummary item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySummaryBudgetRangeRule(double Min, double Max) : IRetroPaySummaryRule
{
    public List<string> Check(RetroPaySummary item)
    {
        var valueValue = item.Budget;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"budget {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"budget {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record RetroPaySummaryRequiredBadgeRule(string Message) : IRetroPaySummaryRule
{
    public List<string> Check(RetroPaySummary item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPaySummarySkillLengthRule(int MaxLength) : IRetroPaySummaryRule
{
    public List<string> Check(RetroPaySummary item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay summary.</summary>
public sealed record RetroPaySummaryValidator(List<IRetroPaySummaryRule> Rules)
{
    public List<string> Validate(RetroPaySummary item)
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

    public void RequireValid(RetroPaySummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPaySummary> items)
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

/// <summary>Functions of retro pay summary rules.</summary>
public static class RetroPaySummaryRules
{
    public static RetroPaySummaryValidator DefaultRetroPaySummaryValidator()
    {
        return new RetroPaySummaryValidator(
            new List<IRetroPaySummaryRule>
            {
                new RetroPaySummaryBadgeLengthRule(64),
                new RetroPaySummaryBudgetRangeRule(2.8, 3246.0),
                new RetroPaySummaryRequiredBadgeRule("retro pay summary needs an entry"),
                new RetroPaySummarySkillLengthRule(80),
            });
    }
}
