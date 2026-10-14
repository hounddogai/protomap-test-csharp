namespace Acme.Workforce.Absence.ReturnPlan.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan summary must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanSummaryRule
{
    List<string> Check(ReturnPlanSummary item);
}

public sealed record ReturnPlanSummaryChannelLengthRule(int MaxLength) : IReturnPlanSummaryRule
{
    public List<string> Check(ReturnPlanSummary item)
    {
        var size = item.Channel.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"channel is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSummaryRequiredExternalRefRule(string Message) : IReturnPlanSummaryRule
{
    public List<string> Check(ReturnPlanSummary item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSummaryGraceDaysRangeRule(int Min, int Max) : IReturnPlanSummaryRule
{
    public List<string> Check(ReturnPlanSummary item)
    {
        var valueValue = item.GraceDays;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"grace days {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"grace days {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanSummaryRequiredNameRule(string Message) : IReturnPlanSummaryRule
{
    public List<string> Check(ReturnPlanSummary item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanSummaryAllowedCodeRule(List<string> Allowed) : IReturnPlanSummaryRule
{
    public List<string> Check(ReturnPlanSummary item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is too long" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan summary.</summary>
public sealed record ReturnPlanSummaryValidator(List<IReturnPlanSummaryRule> Rules)
{
    public List<string> Validate(ReturnPlanSummary item)
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

    public void RequireValid(ReturnPlanSummary item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanSummary> items)
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

/// <summary>Functions of return plan summary rules.</summary>
public static class ReturnPlanSummaryRules
{
    public static ReturnPlanSummaryValidator DefaultReturnPlanSummaryValidator()
    {
        return new ReturnPlanSummaryValidator(
            new List<IReturnPlanSummaryRule>
            {
                new ReturnPlanSummaryChannelLengthRule(86400),
                new ReturnPlanSummaryRequiredExternalRefRule("return plan summary needs this field"),
                new ReturnPlanSummaryGraceDaysRangeRule(16, 120),
                new ReturnPlanSummaryRequiredNameRule("return plan summary needs a value"),
                new ReturnPlanSummaryAllowedCodeRule(new List<string> { "batched", "tiered" }),
            });
    }
}
