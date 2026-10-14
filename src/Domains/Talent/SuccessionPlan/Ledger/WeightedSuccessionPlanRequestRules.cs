namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a weighted succession plan request must pass. Check returns its problems, or none.</summary>
public interface IWeightedSuccessionPlanRequestRule
{
    List<string> Check(WeightedSuccessionPlanRequest item);
}

public sealed record WeightedSuccessionPlanRequestTeamLengthRule(int MaxLength) : IWeightedSuccessionPlanRequestRule
{
    public List<string> Check(WeightedSuccessionPlanRequest item)
    {
        var size = item.Team.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"team is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSuccessionPlanRequestCodeLengthRule(int MaxLength) : IWeightedSuccessionPlanRequestRule
{
    public List<string> Check(WeightedSuccessionPlanRequest item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSuccessionPlanRequestAllowedJobCodeRule(List<string> Allowed) : IWeightedSuccessionPlanRequestRule
{
    public List<string> Check(WeightedSuccessionPlanRequest item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.JobCode)))
        {
            return new List<string> { $"job code {item.JobCode} is out of range" };
        }
        return new List<string>();
    }
}

public sealed record WeightedSuccessionPlanRequestJobCodeLengthRule(int MaxLength) : IWeightedSuccessionPlanRequestRule
{
    public List<string> Check(WeightedSuccessionPlanRequest item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a weighted succession plan request.</summary>
public sealed record WeightedSuccessionPlanRequestValidator(List<IWeightedSuccessionPlanRequestRule> Rules)
{
    public List<string> Validate(WeightedSuccessionPlanRequest item)
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

    public void RequireValid(WeightedSuccessionPlanRequest item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<WeightedSuccessionPlanRequest> items)
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

/// <summary>Functions of weighted succession plan request rules.</summary>
public static class WeightedSuccessionPlanRequestRules
{
    public static WeightedSuccessionPlanRequestValidator DefaultWeightedSuccessionPlanRequestValidator()
    {
        return new WeightedSuccessionPlanRequestValidator(
            new List<IWeightedSuccessionPlanRequestRule>
            {
                new WeightedSuccessionPlanRequestTeamLengthRule(1000),
                new WeightedSuccessionPlanRequestCodeLengthRule(80),
                new WeightedSuccessionPlanRequestAllowedJobCodeRule(new List<string> { "seasonal" }),
                new WeightedSuccessionPlanRequestJobCodeLengthRule(86400),
            });
    }
}
