namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves refresh proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record RefreshProposalDesk(IRefreshProposalStore Store, RefreshProposalValidator Validator, IClock Clock, List<string> Journal, RefreshProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RefreshProposalStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RefreshProposalStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"refresh proposal {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of refresh proposal workflow.</summary>
public static class RefreshProposalWorkflow
{
    public static RefreshProposalDesk NewRefreshProposalDesk(IRefreshProposalStore store, IClock clock, Registry flags)
    {
        return new RefreshProposalDesk(
            store,
            RefreshProposalRules.DefaultRefreshProposalValidator(),
            clock,
            new List<string>(),
            RefreshProposalEngine.DefaultRefreshProposalEngine(flags));
    }
}
