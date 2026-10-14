namespace Acme.Workforce.Engagement.Recognition.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a pending recognition allocation must pass. Check returns its problems, or none.</summary>
public interface IPendingRecognitionAllocationRule
{
    List<string> Check(PendingRecognitionAllocation item);
}

public sealed record PendingRecognitionAllocationAllowedCostCenterRule(List<string> Allowed) : IPendingRecognitionAllocationRule
{
    public List<string> Check(PendingRecognitionAllocation item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.CostCenter)))
        {
            return new List<string> { $"cost center {item.CostCenter} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionAllocationRequiredCostCenterRule(string Message) : IPendingRecognitionAllocationRule
{
    public List<string> Check(PendingRecognitionAllocation item)
    {
        if (item.CostCenter.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionAllocationRequiredLabelRule(string Message) : IPendingRecognitionAllocationRule
{
    public List<string> Check(PendingRecognitionAllocation item)
    {
        if (item.Label.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record PendingRecognitionAllocationCodePrefixRule(string Prefix) : IPendingRecognitionAllocationRule
{
    public List<string> Check(PendingRecognitionAllocation item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a pending recognition allocation.</summary>
public sealed record PendingRecognitionAllocationValidator(List<IPendingRecognitionAllocationRule> Rules)
{
    public List<string> Validate(PendingRecognitionAllocation item)
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

    public void RequireValid(PendingRecognitionAllocation item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PendingRecognitionAllocation> items)
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

/// <summary>Functions of pending recognition allocation rules.</summary>
public static class PendingRecognitionAllocationRules
{
    public static PendingRecognitionAllocationValidator DefaultPendingRecognitionAllocationValidator()
    {
        return new PendingRecognitionAllocationValidator(
            new List<IPendingRecognitionAllocationRule>
            {
                new PendingRecognitionAllocationAllowedCostCenterRule(
                    new List<string> { "seasonal", "manual", "regional" }),
                new PendingRecognitionAllocationRequiredCostCenterRule(
                    "pending recognition allocation needs a value"),
                new PendingRecognitionAllocationRequiredLabelRule("pending recognition allocation needs this field"),
                new PendingRecognitionAllocationCodePrefixRule("ACM"),
            });
    }
}
