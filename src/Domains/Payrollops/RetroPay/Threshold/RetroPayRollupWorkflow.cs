namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves retro pay rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayRollupDesk(IRetroPayRollupStore Store, RetroPayRollupValidator Validator, IClock Clock, List<string> Journal, RetroPayRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayRollupStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayRollupStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of retro pay rollup workflow.</summary>
public static class RetroPayRollupWorkflow
{
    public static RetroPayRollupDesk NewRetroPayRollupDesk(IRetroPayRollupStore store, IClock clock, Registry flags)
    {
        return new RetroPayRollupDesk(
            store,
            RetroPayRollupRules.DefaultRetroPayRollupValidator(),
            clock,
            new List<string>(),
            RetroPayRollupEngine.DefaultRetroPayRollupEngine(flags));
    }
}
