namespace Acme.Workforce.Compensation.EquityGrant.Metric;

/// <summary>A rule a equity grant plan must pass. Check returns its problems, or none.</summary>
public interface IEquityGrantPlanRule
{
    List<string> Check(EquityGrantPlan item);
}

public sealed record EquityGrantPlanCodeLengthRule(int MaxLength) : IEquityGrantPlanRule
{
    public List<string> Check(EquityGrantPlan item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantPlanRequiredTitleRule(string Message) : IEquityGrantPlanRule
{
    public List<string> Check(EquityGrantPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantPlanRetriesRangeRule(int Min, int Max) : IEquityGrantPlanRule
{
    public List<string> Check(EquityGrantPlan item)
    {
        var valueValue = item.Retries;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"retries {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"retries {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record EquityGrantPlanRequiredReviewerRule(string Message) : IEquityGrantPlanRule
{
    public List<string> Check(EquityGrantPlan item)
    {
        if (item.Reviewer.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record EquityGrantPlanTitleLengthRule(int MaxLength) : IEquityGrantPlanRule
{
    public List<string> Check(EquityGrantPlan item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a equity grant plan.</summary>
public sealed record EquityGrantPlanValidator(List<IEquityGrantPlanRule> Rules)
{
    public List<string> Validate(EquityGrantPlan item)
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

    public void RequireValid(EquityGrantPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<EquityGrantPlan> items)
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

/// <summary>Functions of equity grant plan rules.</summary>
public static class EquityGrantPlanRules
{
    public static EquityGrantPlanValidator DefaultEquityGrantPlanValidator()
    {
        return new EquityGrantPlanValidator(
            new List<IEquityGrantPlanRule>
            {
                new EquityGrantPlanCodeLengthRule(3600),
                new EquityGrantPlanRequiredTitleRule("equity grant plan needs an entry"),
                new EquityGrantPlanRetriesRangeRule(80, 255),
                new EquityGrantPlanRequiredReviewerRule("equity grant plan needs this field"),
                new EquityGrantPlanTitleLengthRule(80),
            });
    }
}
