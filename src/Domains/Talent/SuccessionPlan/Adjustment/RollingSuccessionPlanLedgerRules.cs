namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a rolling succession plan ledger must pass. Check returns its problems, or none.</summary>
public interface IRollingSuccessionPlanLedgerRule
{
    List<string> Check(RollingSuccessionPlanLedger item);
}

public sealed record RollingSuccessionPlanLedgerJobCodeLengthRule(int MaxLength) : IRollingSuccessionPlanLedgerRule
{
    public List<string> Check(RollingSuccessionPlanLedger item)
    {
        var size = item.JobCode.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"job code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanLedgerExternalRefLengthRule(int MaxLength) : IRollingSuccessionPlanLedgerRule
{
    public List<string> Check(RollingSuccessionPlanLedger item)
    {
        var size = item.ExternalRef.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"external ref is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanLedgerRequiredExternalRefRule(string Message) : IRollingSuccessionPlanLedgerRule
{
    public List<string> Check(RollingSuccessionPlanLedger item)
    {
        if (item.ExternalRef.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanLedgerCapacityRangeRule(int Min, int Max) : IRollingSuccessionPlanLedgerRule
{
    public List<string> Check(RollingSuccessionPlanLedger item)
    {
        var valueValue = item.Capacity;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"capacity {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record RollingSuccessionPlanLedgerAllowedExternalRefRule(List<string> Allowed) : IRollingSuccessionPlanLedgerRule
{
    public List<string> Check(RollingSuccessionPlanLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.ExternalRef)))
        {
            return new List<string> { $"external ref {item.ExternalRef} is out of range" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a rolling succession plan ledger.</summary>
public sealed record RollingSuccessionPlanLedgerValidator(List<IRollingSuccessionPlanLedgerRule> Rules)
{
    public List<string> Validate(RollingSuccessionPlanLedger item)
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

    public void RequireValid(RollingSuccessionPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<RollingSuccessionPlanLedger> items)
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

/// <summary>Functions of rolling succession plan ledger rules.</summary>
public static class RollingSuccessionPlanLedgerRules
{
    public static RollingSuccessionPlanLedgerValidator DefaultRollingSuccessionPlanLedgerValidator()
    {
        return new RollingSuccessionPlanLedgerValidator(
            new List<IRollingSuccessionPlanLedgerRule>
            {
                new RollingSuccessionPlanLedgerJobCodeLengthRule(80),
                new RollingSuccessionPlanLedgerExternalRefLengthRule(255),
                new RollingSuccessionPlanLedgerRequiredExternalRefRule(
                    "rolling succession plan ledger needs this field"),
                new RollingSuccessionPlanLedgerCapacityRangeRule(16, 16),
                new RollingSuccessionPlanLedgerAllowedExternalRefRule(
                    new List<string> { "adaptive", "legacy", "batched" }),
            });
    }
}
