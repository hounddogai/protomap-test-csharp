namespace Acme.Workforce.Talent.Nomination.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>A rule a nomination ledger must pass. Check returns its problems, or none.</summary>
public interface INominationLedgerRule
{
    List<string> Check(NominationLedger item);
}

public sealed record NominationLedgerArchivedGuardRule(int Limit) : INominationLedgerRule
{
    public List<string> Check(NominationLedger item)
    {
        if (item.Archived && item.Quantity > Limit)
        {
            return new List<string> { $"quantity {item.Quantity} exceeds {Limit} while archived" };
        }
        return new List<string>();
    }
}

public sealed record NominationLedgerAllowedDepartmentRule(List<string> Allowed) : INominationLedgerRule
{
    public List<string> Check(NominationLedger item)
    {
        if (Allowed.Count == 0)
        {
            return new List<string>();
        }
        if (!Allowed.Contains(Text.Normalize(item.Department)))
        {
            return new List<string> { $"department {item.Department} is duplicated" };
        }
        return new List<string>();
    }
}

public sealed record NominationLedgerCodePrefixRule(string Prefix) : INominationLedgerRule
{
    public List<string> Check(NominationLedger item)
    {
        if (!item.Code.ToUpperInvariant().StartsWith(Prefix.ToUpperInvariant(), StringComparison.Ordinal))
        {
            return new List<string> { $"code must start with {Prefix}" };
        }
        return new List<string>();
    }
}

/// <summary>Runs every rule of a nomination ledger.</summary>
public sealed record NominationLedgerValidator(List<INominationLedgerRule> Rules)
{
    public List<string> Validate(NominationLedger item)
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

    public void RequireValid(NominationLedger item)
    {
        var problems = Validate(item);
        if (problems.Count > 0)
        {
            throw new InvalidOperationException($"{item.DisplayLabel()} is invalid: {string.Join("; ", problems)}");
        }
    }

    public Dictionary<string, List<string>> ValidateAll(List<NominationLedger> items)
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

/// <summary>Functions of nomination ledger rules.</summary>
public static class NominationLedgerRules
{
    public static NominationLedgerValidator DefaultNominationLedgerValidator()
    {
        return new NominationLedgerValidator(
            new List<INominationLedgerRule>
            {
                new NominationLedgerArchivedGuardRule(64),
                new NominationLedgerAllowedDepartmentRule(new List<string> { "global" }),
                new NominationLedgerCodePrefixRule("REF"),
            });
    }
}
