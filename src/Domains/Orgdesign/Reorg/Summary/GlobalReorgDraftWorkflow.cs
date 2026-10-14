namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global reorg drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalReorgDraftService(IGlobalReorgDraftStore Store, GlobalReorgDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global reorg draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalReorgDraftStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global reorg draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalReorgDraftStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of global reorg draft workflow.</summary>
public static class GlobalReorgDraftWorkflow
{
    public static GlobalReorgDraftService NewGlobalReorgDraftService(IGlobalReorgDraftStore store, IClock clock)
    {
        return new GlobalReorgDraftService(
            store,
            GlobalReorgDraftRules.DefaultGlobalReorgDraftValidator(),
            clock,
            new List<string>());
    }
}
