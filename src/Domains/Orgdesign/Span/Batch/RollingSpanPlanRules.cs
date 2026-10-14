namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling span plan must pass. Check returns its problems, or none.</summary>
public interface IRollingSpanPlanRule
{
    List<string> Check(RollingSpanPlan item);
}

public sealed record RollingSpanPlanAllowedCodeRule(List<string> Allowed) : IRollingSpanPlanRule
{
    public List<string> Check(RollingSpanPlan item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is inconsistent" };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanPlanOwnerIdLengthRule(int MaxLength) : IRollingSpanPlanRule
{
    public List<string> Check(RollingSpanPlan item)
    {
        var size = item.OwnerId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"owner id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanPlanRequiredCodeRule(string Message) : IRollingSpanPlanRule
{
    public List<string> Check(RollingSpanPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSpanPlanRequiredOwnerIdRule(string Message) : IRollingSpanPlanRule
{
    public List<string> Check(RollingSpanPlan item)
    {
        if (item.OwnerId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling span plan.</summary>
public sealed record RollingSpanPlanValidator(List<IRollingSpanPlanRule> Rules)
{
    public List<string> Validate(RollingSpanPlan item)
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

    public void RequireValid(RollingSpanPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSpanPlan> items)
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

/// <summary>Functions of rolling span plan rules.</summary>
public static class RollingSpanPlanRules
{
    public static RollingSpanPlanValidator DefaultRollingSpanPlanValidator()
    {
        return new RollingSpanPlanValidator(
            new List<IRollingSpanPlanRule>
            {
                new RollingSpanPlanAllowedCodeRule(new List<string> { "lenient", "capped", "adaptive" }),
                new RollingSpanPlanOwnerIdLengthRule(255),
                new RollingSpanPlanRequiredCodeRule("rolling span plan needs a value"),
                new RollingSpanPlanRequiredOwnerIdRule("rolling span plan needs an entry"),
            });
    }
}
