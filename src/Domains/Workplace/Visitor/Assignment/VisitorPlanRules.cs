namespace Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>A rule a visitor plan must pass. Check returns its problems, or none.</summary>
public interface IVisitorPlanRule
{
    List<string> Check(VisitorPlan item);
}

public sealed record VisitorPlanRequiredSourceRule(string Message) : IVisitorPlanRule
{
    public List<string> Check(VisitorPlan item)
    {
        if (item.Source.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorPlanRequiredLabelRule(string Message) : IVisitorPlanRule
{
    public List<string> Check(VisitorPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record VisitorPlanRequiredCodeRule(string Message) : IVisitorPlanRule
{
    public List<string> Check(VisitorPlan item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a visitor plan.</summary>
public sealed record VisitorPlanValidator(List<IVisitorPlanRule> Rules)
{
    public List<string> Validate(VisitorPlan item)
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

    public void RequireValid(VisitorPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<VisitorPlan> items)
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

/// <summary>Functions of visitor plan rules.</summary>
public static class VisitorPlanRules
{
    public static VisitorPlanValidator DefaultVisitorPlanValidator()
    {
        return new VisitorPlanValidator(
            new List<IVisitorPlanRule>
            {
                new VisitorPlanRequiredSourceRule("visitor plan needs a value"),
                new VisitorPlanRequiredLabelRule("visitor plan needs a value"),
                new VisitorPlanRequiredCodeRule("visitor plan needs a value"),
            });
    }
}
