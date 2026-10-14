namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary succession plan request must pass. Check returns its problems, or none.</summary>
public interface IPrimarySuccessionPlanRequestRule
{
    List<string> Check(PrimarySuccessionPlanRequest item);
}

public sealed record PrimarySuccessionPlanRequestTitleLengthRule(int MaxLength) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        var size = item.Title.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"title is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRequestRequiredCodeRule(string Message) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRequestRequiredDescriptionRule(string Message) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        if (item.Description.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRequestRatingRangeRule(double Min, double Max) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        var valueValue = item.Rating;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"rating {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"rating {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimarySuccessionPlanRequestCostRangeRule(double Min, double Max) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        var valueValue = item.Cost;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"cost {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanRequestAllowedTitleRule(List<string> Allowed) : IPrimarySuccessionPlanRequestRule
{
    public List<string> Check(PrimarySuccessionPlanRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is expired" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary succession plan request.</summary>
public sealed record PrimarySuccessionPlanRequestValidator(List<IPrimarySuccessionPlanRequestRule> Rules)
{
    public List<string> Validate(PrimarySuccessionPlanRequest item)
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

    public void RequireValid(PrimarySuccessionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySuccessionPlanRequest> items)
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

/// <summary>Functions of primary succession plan request rules.</summary>
public static class PrimarySuccessionPlanRequestRules
{
    public static PrimarySuccessionPlanRequestValidator DefaultPrimarySuccessionPlanRequestValidator()
    {
        return new PrimarySuccessionPlanRequestValidator(
            new List<IPrimarySuccessionPlanRequestRule>
            {
                new PrimarySuccessionPlanRequestTitleLengthRule(255),
                new PrimarySuccessionPlanRequestRequiredCodeRule("primary succession plan request needs this field"),
                new PrimarySuccessionPlanRequestRequiredDescriptionRule(
                    "primary succession plan request needs a value"),
                new PrimarySuccessionPlanRequestRatingRangeRule(5.8, 4078.9),
                new PrimarySuccessionPlanRequestCostRangeRule(3.4, 4387.9),
                new PrimarySuccessionPlanRequestAllowedTitleRule(new List<string>()),
            });
    }
}
