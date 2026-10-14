namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy span windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacySpanWindowManager(ILegacySpanWindowStore Store, LegacySpanWindowValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy span window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacySpanWindowStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy span window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacySpanWindowStatus.Retired);
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

/// <summary>Functions of legacy span window workflow.</summary>
public static class LegacySpanWindowWorkflow
{
    public static LegacySpanWindowManager NewLegacySpanWindowManager(ILegacySpanWindowStore store, IClock clock)
    {
        return new LegacySpanWindowManager(
            store,
            LegacySpanWindowRules.DefaultLegacySpanWindowValidator(),
            clock,
            new List<string>());
    }
}
