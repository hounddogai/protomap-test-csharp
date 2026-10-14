namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a reorg claim must pass. Check returns its problems, or none.</summary>
public interface IReorgClaimRule
{
    List<string> Check(ReorgClaim item);
}

public sealed record ReorgClaimFloorValueRangeRule(double Min, double Max) : IReorgClaimRule
{
    public List<string> Check(ReorgClaim item)
    {
        var valueValue = item.FloorValue;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"floor value {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgClaimAllowedTeamRule(List<string> Allowed) : IReorgClaimRule
{
    public List<string> Check(ReorgClaim item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Team)))
        {
            return new List<string> { $"team {item.Team} is missing" };
        }
        return new List<string>();
    }
}

public sealed record ReorgClaimMinimumRangeRule(double Min, double Max) : IReorgClaimRule
{
    public List<string> Check(ReorgClaim item)
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

public sealed record ReorgClaimBudgetRangeRule(double Min, double Max) : IReorgClaimRule
{
    public List<string> Check(ReorgClaim item)
    {
        var valueValue = item.Budget;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"budget {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record ReorgClaimRequiredTeamRule(string Message) : IReorgClaimRule
{
    public List<string> Check(ReorgClaim item)
    {
        if (item.Team.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a reorg claim.</summary>
public sealed record ReorgClaimValidator(List<IReorgClaimRule> Rules)
{
    public List<string> Validate(ReorgClaim item)
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

    public void RequireValid(ReorgClaim item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ReorgClaim> items)
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

/// <summary>Functions of reorg claim rules.</summary>
public static class ReorgClaimRules
{
    public static ReorgClaimValidator DefaultReorgClaimValidator()
    {
        return new ReorgClaimValidator(
            new List<IReorgClaimRule>
            {
                new ReorgClaimFloorValueRangeRule(9.6, 2505.9),
                new ReorgClaimAllowedTeamRule(new List<string> { "lenient", "seasonal" }),
                new ReorgClaimMinimumRangeRule(3.8, 3328.7),
                new ReorgClaimBudgetRangeRule(0.4, 4068.8),
                new ReorgClaimRequiredTeamRule("reorg claim needs a value"),
            });
    }
}
