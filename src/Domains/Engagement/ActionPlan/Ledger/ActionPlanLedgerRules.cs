namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a action plan ledger must pass. Check returns its problems, or none.</summary>
public interface IActionPlanLedgerRule
{
    List<string> Check(ActionPlanLedger item);
}

public sealed record ActionPlanLedgerRequiredCodeRule(string Message) : IActionPlanLedgerRule
{
    public List<string> Check(ActionPlanLedger item)
    {
        if (item.Code.Trim() == "")
        {
            return new List<string> { Message };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanLedgerCodePrefixRule(string Prefix) : IActionPlanLedgerRule
{
    public List<string> Check(ActionPlanLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanLedgerCapacityRangeRule(int Min, int Max) : IActionPlanLedgerRule
{
    public List<string> Check(ActionPlanLedger item)
    {
        var valueValue = item.Capacity;
        var problems = new List<string>();
        if (valueValue < Min)
        {
            problems.Add($"capacity {valueValue} is below {Min}");
        }
        if (valueValue > Max)
        {
            problems.Add($"capacity {valueValue} is above {Max}");
        }
        return problems;
    }
}

public sealed record ActionPlanLedgerCodeLengthRule(int MaxLength) : IActionPlanLedgerRule
{
    public List<string> Check(ActionPlanLedger item)
    {
        var size = item.Code.Length;
        if (size > MaxLength)
        {
            return new List<string> { $"code is {size} characters, more than {MaxLength}" };
        }
        return new List<string>();
    }
}

public sealed record ActionPlanLedgerAllowedTeamRule(List<string> Allowed) : IActionPlanLedgerRule
{
    public List<string> Check(ActionPlanLedger item)
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

/// <summary>Runs every rule of a action plan ledger.</summary>
public sealed record ActionPlanLedgerValidator(List<IActionPlanLedgerRule> Rules)
{
    public List<string> Validate(ActionPlanLedger item)
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

    public void RequireValid(ActionPlanLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.Label()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<ActionPlanLedger> items)
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

/// <summary>Functions of action plan ledger rules.</summary>
public static class ActionPlanLedgerRules
{
    public static ActionPlanLedgerValidator DefaultActionPlanLedgerValidator()
    {
        return new ActionPlanLedgerValidator(
            new List<IActionPlanLedgerRule>
            {
                new ActionPlanLedgerRequiredCodeRule("action plan ledger needs a value"),
                new ActionPlanLedgerCodePrefixRule("REF"),
                new ActionPlanLedgerCapacityRangeRule(32, 120),
                new ActionPlanLedgerCodeLengthRule(16),
                new ActionPlanLedgerAllowedTeamRule(new List<string>()),
            });
    }
}
