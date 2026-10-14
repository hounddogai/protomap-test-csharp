namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional balance policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalBalancePolicyService(IRegionalBalancePolicyStore Store, RegionalBalancePolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalBalancePolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional balance policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalBalancePolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public int Audit(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of regional balance policy workflow.</summary>
public static class RegionalBalancePolicyWorkflow
{
    public static RegionalBalancePolicyService NewRegionalBalancePolicyService(IRegionalBalancePolicyStore store, IClock clock)
    {
        return new RegionalBalancePolicyService(
            store,
            RegionalBalancePolicyRules.DefaultRegionalBalancePolicyValidator(),
            clock,
            new List<string>());
    }
}
