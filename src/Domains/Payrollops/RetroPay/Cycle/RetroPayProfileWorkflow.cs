namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayProfileManager(IRetroPayProfileStore Store, RetroPayProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayProfileStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayProfileStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of retro pay profile workflow.</summary>
public static class RetroPayProfileWorkflow
{
    public static RetroPayProfileManager NewRetroPayProfileManager(IRetroPayProfileStore store, IClock clock)
    {
        return new RetroPayProfileManager(
            store,
            RetroPayProfileRules.DefaultRetroPayProfileValidator(),
            clock,
            new List<string>());
    }
}
