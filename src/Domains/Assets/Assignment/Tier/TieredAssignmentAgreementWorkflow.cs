namespace Acme.Workforce.Assets.Assignment.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered assignment agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredAssignmentAgreementDesk(ITieredAssignmentAgreementStore Store, TieredAssignmentAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredAssignmentAgreementStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered assignment agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredAssignmentAgreementStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int ReviewStale(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of tiered assignment agreement workflow.</summary>
public static class TieredAssignmentAgreementWorkflow
{
    public static TieredAssignmentAgreementDesk NewTieredAssignmentAgreementDesk(ITieredAssignmentAgreementStore store, IClock clock)
    {
        return new TieredAssignmentAgreementDesk(
            store,
            TieredAssignmentAgreementRules.DefaultTieredAssignmentAgreementValidator(),
            clock,
            new List<string>());
    }
}
