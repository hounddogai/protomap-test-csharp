namespace Acme.Workforce.Contractors.Offboarding.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a primary offboarding batch must pass. Check returns its problems, or none.</summary>
public interface IPrimaryOffboardingBatchRule
{
    List<string> Check(PrimaryOffboardingBatch item);
}

public sealed record PrimaryOffboardingBatchPriorityRangeRule(int Min, int Max) : IPrimaryOffboardingBatchRule
{
    public List<string> Check(PrimaryOffboardingBatch item)
    {
        var valueValue = item.Priority;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"priority {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingBatchLimitRangeRule(int Min, int Max) : IPrimaryOffboardingBatchRule
{
    public List<string> Check(PrimaryOffboardingBatch item)
    {
        var valueValue = item.Limit;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"limit {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PrimaryOffboardingBatchAllowedCodeRule(List<string> Allowed) : IPrimaryOffboardingBatchRule
{
    public List<string> Check(PrimaryOffboardingBatch item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Code)))
        {
            return new List<string> { $"code {item.Code} is blocked" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a primary offboarding batch.</summary>
public sealed record PrimaryOffboardingBatchValidator(List<IPrimaryOffboardingBatchRule> Rules)
{
    public List<string> Validate(PrimaryOffboardingBatch item)
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

    public void RequireValid(PrimaryOffboardingBatch item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PrimaryOffboardingBatch> items)
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

/// <summary>Functions of primary offboarding batch rules.</summary>
public static class PrimaryOffboardingBatchRules
{
    public static PrimaryOffboardingBatchValidator DefaultPrimaryOffboardingBatchValidator()
    {
        return new PrimaryOffboardingBatchValidator(
            new List<IPrimaryOffboardingBatchRule>
            {
                new PrimaryOffboardingBatchPriorityRangeRule(64, 120),
                new PrimaryOffboardingBatchLimitRangeRule(32, 64),
                new PrimaryOffboardingBatchAllowedCodeRule(new List<string> { "global" }),
            });
    }
}
