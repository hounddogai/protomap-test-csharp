namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient statement of work bundles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientStatementOfWorkBundleCoordinator(ILenientStatementOfWorkBundleStore Store, LenientStatementOfWorkBundleValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientStatementOfWorkBundleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient statement of work bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientStatementOfWorkBundleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of lenient statement of work bundle workflow.</summary>
public static class LenientStatementOfWorkBundleWorkflow
{
    public static LenientStatementOfWorkBundleCoordinator NewLenientStatementOfWorkBundleCoordinator(ILenientStatementOfWorkBundleStore store, IClock clock)
    {
        return new LenientStatementOfWorkBundleCoordinator(
            store,
            LenientStatementOfWorkBundleRules.DefaultLenientStatementOfWorkBundleValidator(),
            clock,
            new List<string>());
    }
}
