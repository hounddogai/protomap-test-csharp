namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy reorg summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyReorgSummaryManager(ILegacyReorgSummaryStore Store, LegacyReorgSummaryValidator Validator, IClock Clock, List<string> Journal, LegacyReorgSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy reorg summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyReorgSummaryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy reorg summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyReorgSummaryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy reorg summary {code} not found");
        }
        var result = Engine.Price(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of legacy reorg summary workflow.</summary>
public static class LegacyReorgSummaryWorkflow
{
    public static LegacyReorgSummaryManager NewLegacyReorgSummaryManager(ILegacyReorgSummaryStore store, IClock clock)
    {
        return new LegacyReorgSummaryManager(
            store,
            LegacyReorgSummaryRules.DefaultLegacyReorgSummaryValidator(),
            clock,
            new List<string>(),
            LegacyReorgSummaryEngine.DefaultLegacyReorgSummaryEngine());
    }
}
