namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling reorg summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingReorgSummaryManager(IRollingReorgSummaryStore Store, RollingReorgSummaryValidator Validator, IClock Clock, List<string> Journal, RollingReorgSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling reorg summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingReorgSummaryStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling reorg summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingReorgSummaryStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling reorg summary {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of rolling reorg summary workflow.</summary>
public static class RollingReorgSummaryWorkflow
{
    public static RollingReorgSummaryManager NewRollingReorgSummaryManager(IRollingReorgSummaryStore store, IClock clock, Registry flags)
    {
        return new RollingReorgSummaryManager(
            store,
            RollingReorgSummaryRules.DefaultRollingReorgSummaryValidator(),
            clock,
            new List<string>(),
            RollingReorgSummaryEngine.DefaultRollingReorgSummaryEngine(flags));
    }
}
