namespace Acme.Workforce.Talent.SuccessionPlan.Quota;

/// <summary>A rule a rolling succession plan request must pass. Check returns its problems, or none.</summary>
public interface IRollingSuccessionPlanRequestRule
{
    List<string> Check(RollingSuccessionPlanRequest item);
}

public sealed record RollingSuccessionPlanRequestRateRangeRule(double Min, double Max) : IRollingSuccessionPlanRequestRule
{
    public List<string> Check(RollingSuccessionPlanRequest item)
    {
        var valueValue = item.Rate;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"rate {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanRequestBadgeLengthRule(int MaxLength) : IRollingSuccessionPlanRequestRule
{
    public List<string> Check(RollingSuccessionPlanRequest item)
    {
        var size = item.Badge.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"badge is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanRequestRequiredBadgeRule(string Message) : IRollingSuccessionPlanRequestRule
{
    public List<string> Check(RollingSuccessionPlanRequest item)
    {
        if (item.Badge.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanRequestRequiredNameRule(string Message) : IRollingSuccessionPlanRequestRule
{
    public List<string> Check(RollingSuccessionPlanRequest item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling succession plan request.</summary>
public sealed record RollingSuccessionPlanRequestValidator(List<IRollingSuccessionPlanRequestRule> Rules)
{
    public List<string> Validate(RollingSuccessionPlanRequest item)
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

    public void RequireValid(RollingSuccessionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSuccessionPlanRequest> items)
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

/// <summary>Functions of rolling succession plan request rules.</summary>
public static class RollingSuccessionPlanRequestRules
{
    public static RollingSuccessionPlanRequestValidator DefaultRollingSuccessionPlanRequestValidator()
    {
        return new RollingSuccessionPlanRequestValidator(
            new List<IRollingSuccessionPlanRequestRule>
            {
                new RollingSuccessionPlanRequestRateRangeRule(0.6, 4839.7),
                new RollingSuccessionPlanRequestBadgeLengthRule(3600),
                new RollingSuccessionPlanRequestRequiredBadgeRule("rolling succession plan request needs an entry"),
                new RollingSuccessionPlanRequestRequiredNameRule("rolling succession plan request needs a value"),
            });
    }
}
