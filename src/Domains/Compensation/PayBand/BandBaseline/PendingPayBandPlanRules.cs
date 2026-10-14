namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

/// <summary>A rule a pending pay band plan must pass. Check returns its problems, or none.</summary>
public interface IPendingPayBandPlanRule
{
    List<string> Check(PendingPayBandPlan item);
}

public sealed record PendingPayBandPlanCostRangeRule(double Min, double Max) : IPendingPayBandPlanRule
{
    public List<string> Check(PendingPayBandPlan item)
    {
        var valueValue = item.Cost;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"cost {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"cost {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record PendingPayBandPlanCodePrefixRule(string Prefix) : IPendingPayBandPlanRule
{
    public List<string> Check(PendingPayBandPlan item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandPlanSkillLengthRule(int MaxLength) : IPendingPayBandPlanRule
{
    public List<string> Check(PendingPayBandPlan item)
    {
        var size = item.Skill.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"skill is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PendingPayBandPlanRequiredTitleRule(string Message) : IPendingPayBandPlanRule
{
    public List<string> Check(PendingPayBandPlan item)
    {
        if (item.Title.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending pay band plan.</summary>
public sealed record PendingPayBandPlanValidator(List<IPendingPayBandPlanRule> Rules)
{
    public List<string> Validate(PendingPayBandPlan item)
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

    public void RequireValid(PendingPayBandPlan item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingPayBandPlan> items)
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

/// <summary>Functions of pending pay band plan rules.</summary>
public static class PendingPayBandPlanRules
{
    public static PendingPayBandPlanValidator DefaultPendingPayBandPlanValidator()
    {
        return new PendingPayBandPlanValidator(
            new List<IPendingPayBandPlanRule>
            {
                new PendingPayBandPlanCostRangeRule(3.3, 4517.6),
                new PendingPayBandPlanCodePrefixRule("REF"),
                new PendingPayBandPlanSkillLengthRule(120),
                new PendingPayBandPlanRequiredTitleRule("pending pay band plan needs an entry"),
            });
    }
}
