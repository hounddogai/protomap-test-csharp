namespace Acme.Workforce.Absence.Accrual.OverrideItem;

/// <summary>A rule a primary accrual plan must pass. Check returns its problems, or none.</summary>
public interface IPrimaryAccrualPlanRule
{
    List<string> Check(PrimaryAccrualPlan item);
}

public sealed record PrimaryAccrualPlanRequiredCountryRule(string Message) : IPrimaryAccrualPlanRule
{
    public List<string> Check(PrimaryAccrualPlan item)
    {
        if (item.Country.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualPlanCodePrefixRule(string Prefix) : IPrimaryAccrualPlanRule
{
    public List<string> Check(PrimaryAccrualPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryAccrualPlanRequiredLabelRule(string Message) : IPrimaryAccrualPlanRule
{
    public List<string> Check(PrimaryAccrualPlan item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary accrual plan.</summary>
public sealed record PrimaryAccrualPlanValidator(List<IPrimaryAccrualPlanRule> Rules)
{
    public List<string> Validate(PrimaryAccrualPlan item)
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

    public void RequireValid(PrimaryAccrualPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryAccrualPlan> items)
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

/// <summary>Functions of primary accrual plan rules.</summary>
public static class PrimaryAccrualPlanRules
{
    public static PrimaryAccrualPlanValidator DefaultPrimaryAccrualPlanValidator()
    {
        return new PrimaryAccrualPlanValidator(
            new List<IPrimaryAccrualPlanRule>
            {
                new PrimaryAccrualPlanRequiredCountryRule("primary accrual plan needs this field"),
                new PrimaryAccrualPlanCodePrefixRule("X"),
                new PrimaryAccrualPlanRequiredLabelRule("primary accrual plan needs a value"),
            });
    }
}
