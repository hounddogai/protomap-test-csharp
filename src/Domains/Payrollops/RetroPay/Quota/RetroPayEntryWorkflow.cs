namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay entries through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayEntryService(IRetroPayEntryStore Store, RetroPayEntryValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayEntryStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayEntryStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Scan(int cutoff)
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

/// <summary>Functions of retro pay entry workflow.</summary>
public static class RetroPayEntryWorkflow
{
    public static RetroPayEntryService NewRetroPayEntryService(IRetroPayEntryStore store, IClock clock)
    {
        return new RetroPayEntryService(
            store,
            RetroPayEntryRules.DefaultRetroPayEntryValidator(),
            clock,
            new List<string>());
    }
}
