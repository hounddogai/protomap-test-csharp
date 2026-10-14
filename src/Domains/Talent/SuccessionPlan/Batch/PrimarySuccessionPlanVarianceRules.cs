namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

/// <summary>A rule a primary succession plan variance must pass. Check returns its problems, or none.</summary>
public interface IPrimarySuccessionPlanVarianceRule
{
    List<string> Check(PrimarySuccessionPlanVariance item);
}

public sealed record PrimarySuccessionPlanVarianceRequiredCodeRule(string Message) : IPrimarySuccessionPlanVarianceRule
{
    public List<string> Check(PrimarySuccessionPlanVariance item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanVarianceRequiredTitleRule(string Message) : IPrimarySuccessionPlanVarianceRule
{
    public List<string> Check(PrimarySuccessionPlanVariance item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PrimarySuccessionPlanVarianceUpdatedAtRangeRule(int Min, int Max) : IPrimarySuccessionPlanVarianceRule
{
    public List<string> Check(PrimarySuccessionPlanVariance item)
    {
        var valueValue = item.UpdatedAt;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"updated at {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"updated at {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PrimarySuccessionPlanVarianceRequiredUnitRule(string Message) : IPrimarySuccessionPlanVarianceRule
{
    public List<string> Check(PrimarySuccessionPlanVariance item)
    {
        if (item.Unit.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary succession plan variance.</summary>
public sealed record PrimarySuccessionPlanVarianceValidator(List<IPrimarySuccessionPlanVarianceRule> Rules)
{
    public List<string> Validate(PrimarySuccessionPlanVariance item)
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

    public void RequireValid(PrimarySuccessionPlanVariance item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimarySuccessionPlanVariance> items)
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

/// <summary>Functions of primary succession plan variance rules.</summary>
public static class PrimarySuccessionPlanVarianceRules
{
    public static PrimarySuccessionPlanVarianceValidator DefaultPrimarySuccessionPlanVarianceValidator()
    {
        return new PrimarySuccessionPlanVarianceValidator(
            new List<IPrimarySuccessionPlanVarianceRule>
            {
                new PrimarySuccessionPlanVarianceRequiredCodeRule(
                    "primary succession plan variance needs this field"),
                new PrimarySuccessionPlanVarianceRequiredTitleRule(
                    "primary succession plan variance needs this field"),
                new PrimarySuccessionPlanVarianceUpdatedAtRangeRule(16, 3600),
                new PrimarySuccessionPlanVarianceRequiredUnitRule("primary succession plan variance needs a value"),
            });
    }
}
