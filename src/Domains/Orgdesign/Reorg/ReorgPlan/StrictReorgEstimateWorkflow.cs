namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict reorg estimates through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictReorgEstimateDesk(IStrictReorgEstimateStore Store, StrictReorgEstimateValidator Validator, IClock Clock, List<string> Journal, StrictReorgEstimateEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg estimate {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictReorgEstimateStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg estimate {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictReorgEstimateStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg estimate {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of strict reorg estimate workflow.</summary>
public static class StrictReorgEstimateWorkflow
{
    public static StrictReorgEstimateDesk NewStrictReorgEstimateDesk(IStrictReorgEstimateStore store, IClock clock, Registry flags)
    {
        return new StrictReorgEstimateDesk(
            store,
            StrictReorgEstimateRules.DefaultStrictReorgEstimateValidator(),
            clock,
            new List<string>(),
            StrictReorgEstimateEngine.DefaultStrictReorgEstimateEngine(flags));
    }
}
