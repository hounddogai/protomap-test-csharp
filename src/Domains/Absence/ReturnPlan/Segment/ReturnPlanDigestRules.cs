namespace Acme.Workforce.Absence.ReturnPlan.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a return plan digest must pass. Check returns its problems, or none.</summary>
public interface IReturnPlanDigestRule
{
    List<string> Check(ReturnPlanDigest item);
}

public sealed record ReturnPlanDigestRegionLengthRule(int MaxLength) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        var size = item.Region.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"region is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDigestRequiredRegionRule(string Message) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        if (item.Region.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDigestMarginRangeRule(double Min, double Max) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        var valueValue = item.Margin;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"margin {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"margin {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ReturnPlanDigestAllowedTeamRule(List<string> Allowed) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is too long" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDigestNameLengthRule(int MaxLength) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReturnPlanDigestRequiredTeamRule(string Message) : IReturnPlanDigestRule
{
    public List<string> Check(ReturnPlanDigest item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a return plan digest.</summary>
public sealed record ReturnPlanDigestValidator(List<IReturnPlanDigestRule> Rules)
{
    public List<string> Validate(ReturnPlanDigest item)
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

    public void RequireValid(ReturnPlanDigest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReturnPlanDigest> items)
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

/// <summary>Functions of return plan digest rules.</summary>
public static class ReturnPlanDigestRules
{
    public static ReturnPlanDigestValidator DefaultReturnPlanDigestValidator()
    {
        return new ReturnPlanDigestValidator(
            new List<IReturnPlanDigestRule>
            {
                new ReturnPlanDigestRegionLengthRule(16),
                new ReturnPlanDigestRequiredRegionRule("return plan digest needs a value"),
                new ReturnPlanDigestMarginRangeRule(6.7, 3062.5),
                new ReturnPlanDigestAllowedTeamRule(new List<string> { "regional", "default" }),
                new ReturnPlanDigestNameLengthRule(16),
                new ReturnPlanDigestRequiredTeamRule("return plan digest needs this field"),
            });
    }
}
