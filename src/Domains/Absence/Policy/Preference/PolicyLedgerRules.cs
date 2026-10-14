namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a policy ledger must pass. Check returns its problems, or none.</summary>
public interface IPolicyLedgerRule
{
    List<string> Check(PolicyLedger item);
}

public sealed record PolicyLedgerEndsAtRangeRule(int Min, int Max) : IPolicyLedgerRule
{
    public List<string> Check(PolicyLedger item)
    {
        var valueValue = item.EndsAt;
        if (valueValue < Min || valueValue > Max)
        {
            return new List<string> { $"ends at {valueValue} is outside {Min} to {Max}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyLedgerAllowedTitleRule(List<string> Allowed) : IPolicyLedgerRule
{
    public List<string> Check(PolicyLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Title)))
        {
            return new List<string> { $"title {item.Title} is above maximum" };
        }
        return new List<string>();
    }
}

public sealed record PolicyLedgerCodeLengthRule(int MaxLength) : IPolicyLedgerRule
{
    public List<string> Check(PolicyLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record PolicyLedgerCostRangeRule(double Min, double Max) : IPolicyLedgerRule
{
    public List<string> Check(PolicyLedger item)
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

/// <summary>Runs every rule of a policy ledger.</summary>
public sealed record PolicyLedgerValidator(List<IPolicyLedgerRule> Rules)
{
    public List<string> Validate(PolicyLedger item)
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

    public void RequireValid(PolicyLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<PolicyLedger> items)
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

/// <summary>Functions of policy ledger rules.</summary>
public static class PolicyLedgerRules
{
    public static PolicyLedgerValidator DefaultPolicyLedgerValidator()
    {
        return new PolicyLedgerValidator(
            new List<IPolicyLedgerRule>
            {
                new PolicyLedgerEndsAtRangeRule(16, 120),
                new PolicyLedgerAllowedTitleRule(new List<string>()),
                new PolicyLedgerCodeLengthRule(80),
                new PolicyLedgerCostRangeRule(4.3, 1744.3),
            });
    }
}
