namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountDraftCoordinator(IHeadcountDraftStore Store, HeadcountDraftValidator Validator, IClock Clock, List<string> Journal, HeadcountDraftEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountDraftStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountDraftStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount draft {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of headcount draft workflow.</summary>
public static class HeadcountDraftWorkflow
{
    public static HeadcountDraftCoordinator NewHeadcountDraftCoordinator(IHeadcountDraftStore store, IClock clock, Registry flags)
    {
        return new HeadcountDraftCoordinator(
            store,
            HeadcountDraftRules.DefaultHeadcountDraftValidator(),
            clock,
            new List<string>(),
            HeadcountDraftEngine.DefaultHeadcountDraftEngine(flags));
    }
}
