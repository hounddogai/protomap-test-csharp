namespace Acme.Workforce.Payrollops.RetroPay.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay drafts through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayDraftDesk(IRetroPayDraftStore Store, RetroPayDraftValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay draft {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayDraftStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay draft {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayDraftStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of retro pay draft workflow.</summary>
public static class RetroPayDraftWorkflow
{
    public static RetroPayDraftDesk NewRetroPayDraftDesk(IRetroPayDraftStore store, IClock clock)
    {
        return new RetroPayDraftDesk(
            store,
            RetroPayDraftRules.DefaultRetroPayDraftValidator(),
            clock,
            new List<string>());
    }
}
