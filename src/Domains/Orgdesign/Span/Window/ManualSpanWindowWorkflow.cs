namespace Acme.Workforce.Orgdesign.Span.Window;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual span windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualSpanWindowService(IManualSpanWindowStore Store, ManualSpanWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual span window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualSpanWindowStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual span window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualSpanWindowStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual span window workflow.</summary>
public static class ManualSpanWindowWorkflow
{
    public static ManualSpanWindowService NewManualSpanWindowService(IManualSpanWindowStore store, IClock clock)
    {
        return new ManualSpanWindowService(
            store,
            ManualSpanWindowRules.DefaultManualSpanWindowValidator(),
            clock,
            new List<string>());
    }
}
