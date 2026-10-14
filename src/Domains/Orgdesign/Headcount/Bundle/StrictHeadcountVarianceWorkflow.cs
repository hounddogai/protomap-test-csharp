namespace Acme.Workforce.Orgdesign.Headcount.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves strict headcount variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictHeadcountVarianceService(IStrictHeadcountVarianceStore Store, StrictHeadcountVarianceValidator Validator, IClock Clock, List<string> Journal, StrictHeadcountVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict headcount variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictHeadcountVarianceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict headcount variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictHeadcountVarianceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict headcount variance {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of strict headcount variance workflow.</summary>
public static class StrictHeadcountVarianceWorkflow
{
    public static StrictHeadcountVarianceService NewStrictHeadcountVarianceService(IStrictHeadcountVarianceStore store, IClock clock, Registry flags)
    {
        return new StrictHeadcountVarianceService(
            store,
            StrictHeadcountVarianceRules.DefaultStrictHeadcountVarianceValidator(),
            clock,
            new List<string>(),
            StrictHeadcountVarianceEngine.DefaultStrictHeadcountVarianceEngine(flags));
    }
}
