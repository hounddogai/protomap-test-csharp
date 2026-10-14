namespace Acme.Workforce.Absence.ReturnPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan bundle must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanBundleRule
{
    List<string> Check(ReturnPlanBundle item);
}

public sealed record ReturnPlanBundleAllowedSourceRule(List<string> Allowed) : IReturnPlanBundleRule
{
    public List<string> Check(ReturnPlanBundle item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Source)))
        {
            return new List<string> { $"source {item.Source} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanBundleMinimumRangeRule(double Min, double Max) : IReturnPlanBundleRule
{
    public List<string> Check(ReturnPlanBundle item)
    {
        var valueValue = item.Minimum;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"minimum {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"minimum {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanBundleFteRangeRule(double Min, double Max) : IReturnPlanBundleRule
{
    public List<string> Check(ReturnPlanBundle item)
    {
        var valueValue = item.Fte;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"fte {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"fte {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanBundleContactEmailLengthRule(int MaxLength) : IReturnPlanBundleRule
{
    public List<string> Check(ReturnPlanBundle item)
    {
        var size = item.ContactEmail.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"contact email is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan bundle.</summary>
public sealed record ReturnPlanBundleValidator(List<IReturnPlanBundleRule> Rules)
{
    public List<string> Validate(ReturnPlanBundle item)
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

    public void RequireValid(ReturnPlanBundle item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanBundle> items)
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

/// <summary>Functions of return plan bundle rules.</summary>
public static class ReturnPlanBundleRules
{
    public static ReturnPlanBundleValidator DefaultReturnPlanBundleValidator()
    {
        return new ReturnPlanBundleValidator(
            new List<IReturnPlanBundleRule>
            {
                new ReturnPlanBundleAllowedSourceRule(new List<string> { "capped" }),
                new ReturnPlanBundleMinimumRangeRule(2.6, 282.2),
                new ReturnPlanBundleFteRangeRule(2.6, 85.8),
                new ReturnPlanBundleContactEmailLengthRule(3600),
            });
    }
}
