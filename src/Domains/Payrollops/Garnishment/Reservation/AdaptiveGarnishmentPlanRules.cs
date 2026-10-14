namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

/// <summary>A rule a adaptive garnishment plan must pass. Check returns its problems, or none.</summary>
public interface IAdaptiveGarnishmentPlanRule
{
    List<string> Check(AdaptiveGarnishmentPlan item);
}

public sealed record AdaptiveGarnishmentPlanRequiredEmployeeIdRule(string Message) : IAdaptiveGarnishmentPlanRule
{
    public List<string> Check(AdaptiveGarnishmentPlan item)
    {
        if (item.EmployeeId.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentPlanMaximumRangeRule(double Min, double Max) : IAdaptiveGarnishmentPlanRule
{
    public List<string> Check(AdaptiveGarnishmentPlan item)
    {
        var valueValue = item.Maximum;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"maximum {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentPlanFteRangeRule(double Min, double Max) : IAdaptiveGarnishmentPlanRule
{
    public List<string> Check(AdaptiveGarnishmentPlan item)
    {
        var valueValue = item.Fte;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"fte {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record AdaptiveGarnishmentPlanRequiredTitleRule(string Message) : IAdaptiveGarnishmentPlanRule
{
    public List<string> Check(AdaptiveGarnishmentPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a adaptive garnishment plan.</summary>
public sealed record AdaptiveGarnishmentPlanValidator(List<IAdaptiveGarnishmentPlanRule> Rules)
{
    public List<string> Validate(AdaptiveGarnishmentPlan item)
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

    public void RequireValid(AdaptiveGarnishmentPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<AdaptiveGarnishmentPlan> items)
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

/// <summary>Functions of adaptive garnishment plan rules.</summary>
public static class AdaptiveGarnishmentPlanRules
{
    public static AdaptiveGarnishmentPlanValidator DefaultAdaptiveGarnishmentPlanValidator()
    {
        return new AdaptiveGarnishmentPlanValidator(
            new List<IAdaptiveGarnishmentPlanRule>
            {
                new AdaptiveGarnishmentPlanRequiredEmployeeIdRule("adaptive garnishment plan needs a value"),
                new AdaptiveGarnishmentPlanMaximumRangeRule(5.1, 1864.3),
                new AdaptiveGarnishmentPlanFteRangeRule(7.8, 4217.6),
                new AdaptiveGarnishmentPlanRequiredTitleRule("adaptive garnishment plan needs this field"),
            });
    }
}
