namespace Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>A rule a reorg plan must pass. Check returns its problems, or none.</summary>
public interface IReorgPlanRule
{
    List<string> Check(ReorgPlan item);
}

public sealed record ReorgPlanCodePrefixRule(string Prefix) : IReorgPlanRule
{
    public List<string> Check(ReorgPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPlanRequiredEmployeeIdRule(string Message) : IReorgPlanRule
{
    public List<string> Check(ReorgPlan item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ReorgPlanNameLengthRule(int MaxLength) : IReorgPlanRule
{
    public List<string> Check(ReorgPlan item)
    {
        var size = item.Name.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"name is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgPlanEmployeeIdLengthRule(int MaxLength) : IReorgPlanRule
{
    public List<string> Check(ReorgPlan item)
    {
        var size = item.EmployeeId.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"employee id is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg plan.</summary>
public sealed record ReorgPlanValidator(List<IReorgPlanRule> Rules)
{
    public List<string> Validate(ReorgPlan item)
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

    public void RequireValid(ReorgPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgPlan> items)
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

/// <summary>Functions of reorg plan rules.</summary>
public static class ReorgPlanRules
{
    public static ReorgPlanValidator DefaultReorgPlanValidator()
    {
        return new ReorgPlanValidator(
            new List<IReorgPlanRule>
            {
                new ReorgPlanCodePrefixRule("Q"),
                new ReorgPlanRequiredEmployeeIdRule("reorg plan needs a value"),
                new ReorgPlanNameLengthRule(3600),
                new ReorgPlanEmployeeIdLengthRule(255),
            });
    }
}
