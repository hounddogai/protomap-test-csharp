namespace Acme.Workforce.Compensation.PayBand.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pay band rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PayBandRollupWorkflow(IPayBandRollupStore Store, PayBandRollupValidator Validator, IClock Clock, List<string> Journal, PayBandRollupEngine Engine)
{
    public static PayBandRollupWorkflow NewPayBandRollupWorkflow(IPayBandRollupStore store, IClock clock, Registry flags)
    {
        return new PayBandRollupWorkflow(
            store,
            PayBandRollupRules.DefaultPayBandRollupValidator(),
            clock,
            new List<string>(),
            PayBandRollupEngine.DefaultPayBandRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PayBandRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PayBandRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pay band rollup {code} not found");
        }
        var result = Engine.Evaluate(item);
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
