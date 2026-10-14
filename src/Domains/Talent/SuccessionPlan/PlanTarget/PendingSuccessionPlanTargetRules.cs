namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>A rule a pending succession plan target must pass. Check returns its problems, or none.</summary>
public interface IPendingSuccessionPlanTargetRule
{
    List<string> Check(PendingSuccessionPlanTarget item);
}

public sealed record PendingSuccessionPlanTargetCodePrefixRule(string Prefix) : IPendingSuccessionPlanTargetRule
{
    public List<string> Check(PendingSuccessionPlanTarget item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record PendingSuccessionPlanTargetRequiredLabelRule(string Message) : IPendingSuccessionPlanTargetRule
{
    public List<string> Check(PendingSuccessionPlanTarget item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingSuccessionPlanTargetCodeLengthRule(int MaxLength) : IPendingSuccessionPlanTargetRule
{
    public List<string> Check(PendingSuccessionPlanTarget item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending succession plan target.</summary>
public sealed record PendingSuccessionPlanTargetValidator(List<IPendingSuccessionPlanTargetRule> Rules)
{
    public List<string> Validate(PendingSuccessionPlanTarget item)
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

    public void RequireValid(PendingSuccessionPlanTarget item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingSuccessionPlanTarget> items)
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

/// <summary>Functions of pending succession plan target rules.</summary>
public static class PendingSuccessionPlanTargetRules
{
    public static PendingSuccessionPlanTargetValidator DefaultPendingSuccessionPlanTargetValidator()
    {
        return new PendingSuccessionPlanTargetValidator(
            new List<IPendingSuccessionPlanTargetRule>
            {
                new PendingSuccessionPlanTargetCodePrefixRule("X"),
                new PendingSuccessionPlanTargetRequiredLabelRule("pending succession plan target needs this field"),
                new PendingSuccessionPlanTargetCodeLengthRule(3600),
            });
    }
}
