namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>A rule a retro pay plan must pass. Check returns its problems, or none.</summary>
public interface IRetroPayPlanRule
{
    List<string> Check(RetroPayPlan item);
}

public sealed record RetroPayPlanRequiredNameRule(string Message) : IRetroPayPlanRule
{
    public List<string> Check(RetroPayPlan item)
    {
        if (item.Name.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayPlanRequiredCodeRule(string Message) : IRetroPayPlanRule
{
    public List<string> Check(RetroPayPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RetroPayPlanCodePrefixRule(string Prefix) : IRetroPayPlanRule
{
    public List<string> Check(RetroPayPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a retro pay plan.</summary>
public sealed record RetroPayPlanValidator(List<IRetroPayPlanRule> Rules)
{
    public List<string> Validate(RetroPayPlan item)
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

    public void RequireValid(RetroPayPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RetroPayPlan> items)
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

/// <summary>Functions of retro pay plan rules.</summary>
public static class RetroPayPlanRules
{
    public static RetroPayPlanValidator DefaultRetroPayPlanValidator()
    {
        return new RetroPayPlanValidator(
            new List<IRetroPayPlanRule>
            {
                new RetroPayPlanRequiredNameRule("retro pay plan needs a value"),
                new RetroPayPlanRequiredCodeRule("retro pay plan needs this field"),
                new RetroPayPlanCodePrefixRule("X"),
            });
    }
}
