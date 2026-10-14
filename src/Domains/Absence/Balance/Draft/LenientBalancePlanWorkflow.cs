namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient balance plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBalancePlanManager(ILenientBalancePlanStore Store, LenientBalancePlanValidator Validator, IClock Clock, List<string> Journal, LenientBalancePlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBalancePlanStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBalancePlanStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance plan {code} not found");
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

/// <summary>Functions of lenient balance plan workflow.</summary>
public static class LenientBalancePlanWorkflow
{
    public static LenientBalancePlanManager NewLenientBalancePlanManager(ILenientBalancePlanStore store, IClock clock, Registry flags)
    {
        return new LenientBalancePlanManager(
            store,
            LenientBalancePlanRules.DefaultLenientBalancePlanValidator(),
            clock,
            new List<string>(),
            LenientBalancePlanEngine.DefaultLenientBalancePlanEngine(flags));
    }
}
