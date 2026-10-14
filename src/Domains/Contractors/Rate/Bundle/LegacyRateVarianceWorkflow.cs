namespace Acme.Workforce.Contractors.Rate.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy rate variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyRateVarianceDesk(ILegacyRateVarianceStore Store, LegacyRateVarianceValidator Validator, IClock Clock, List<string> Journal, LegacyRateVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy rate variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyRateVarianceStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy rate variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyRateVarianceStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy rate variance {code} not found");
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

/// <summary>Functions of legacy rate variance workflow.</summary>
public static class LegacyRateVarianceWorkflow
{
    public static LegacyRateVarianceDesk NewLegacyRateVarianceDesk(ILegacyRateVarianceStore store, IClock clock, Registry flags)
    {
        return new LegacyRateVarianceDesk(
            store,
            LegacyRateVarianceRules.DefaultLegacyRateVarianceValidator(),
            clock,
            new List<string>(),
            LegacyRateVarianceEngine.DefaultLegacyRateVarianceEngine(flags));
    }
}
