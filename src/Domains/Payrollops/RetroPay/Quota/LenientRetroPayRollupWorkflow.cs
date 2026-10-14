namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient retro pay rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientRetroPayRollupWorkflow(ILenientRetroPayRollupStore Store, LenientRetroPayRollupValidator Validator, IClock Clock, List<string> Journal, LenientRetroPayRollupEngine Engine)
{
    public static LenientRetroPayRollupWorkflow NewLenientRetroPayRollupWorkflow(ILenientRetroPayRollupStore store, IClock clock, Registry flags)
    {
        return new LenientRetroPayRollupWorkflow(
            store,
            LenientRetroPayRollupRules.DefaultLenientRetroPayRollupValidator(),
            clock,
            new List<string>(),
            LenientRetroPayRollupEngine.DefaultLenientRetroPayRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientRetroPayRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientRetroPayRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient retro pay rollup {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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
