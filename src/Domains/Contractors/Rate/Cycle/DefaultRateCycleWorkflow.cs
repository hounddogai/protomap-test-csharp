namespace Acme.Workforce.Contractors.Rate.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default rate cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultRateCycleDesk(IDefaultRateCycleStore Store, DefaultRateCycleValidatorExtra Validator, IClock Clock, List<string> Journal, DefaultRateCycleEngineExtra Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultRateCycleStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultRateCycleStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default rate cycle {code} not found");
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

/// <summary>Functions of default rate cycle workflow.</summary>
public static class DefaultRateCycleWorkflow
{
    public static DefaultRateCycleDesk NewDefaultRateCycleDesk(IDefaultRateCycleStore store, IClock clock, Registry flags)
    {
        return new DefaultRateCycleDesk(
            store,
            DefaultRateCycleRules.DefaultDefaultRateCycleValidator(),
            clock,
            new List<string>(),
            DefaultRateCycleEngine.DefaultDefaultRateCycleEngine(flags));
    }
}
