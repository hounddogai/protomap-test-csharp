namespace Acme.Workforce.Talent.Pool.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global pool cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalPoolCycleWorkflow(IGlobalPoolCycleStore Store, GlobalPoolCycleValidator Validator, IClock Clock, List<string> Journal, GlobalPoolCycleEngine Engine)
{
    public static GlobalPoolCycleWorkflow NewGlobalPoolCycleWorkflow(IGlobalPoolCycleStore store, IClock clock)
    {
        return new GlobalPoolCycleWorkflow(
            store,
            GlobalPoolCycleRules.DefaultGlobalPoolCycleValidator(),
            clock,
            new List<string>(),
            GlobalPoolCycleEngine.DefaultGlobalPoolCycleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pool cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalPoolCycleStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pool cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalPoolCycleStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global pool cycle {code} not found");
        }
        var result = Engine.Run(item);
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
