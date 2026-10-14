namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves access badge agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record AccessBadgeAgreementService(IAccessBadgeAgreementStore Store, AccessBadgeAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AccessBadgeAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"access badge agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AccessBadgeAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of access badge agreement workflow.</summary>
public static class AccessBadgeAgreementWorkflow
{
    public static AccessBadgeAgreementService NewAccessBadgeAgreementService(IAccessBadgeAgreementStore store, IClock clock)
    {
        return new AccessBadgeAgreementService(
            store,
            AccessBadgeAgreementRules.DefaultAccessBadgeAgreementValidator(),
            clock,
            new List<string>());
    }
}
