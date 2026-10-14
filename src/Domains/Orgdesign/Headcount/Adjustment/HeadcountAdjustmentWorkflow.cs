namespace Acme.Workforce.Orgdesign.Headcount.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves headcount adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountAdjustmentDesk(IHeadcountAdjustmentStore Store, HeadcountAdjustmentValidator Validator, IClock Clock, List<string> Journal, HeadcountAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountAdjustmentStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountAdjustmentStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount adjustment {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of headcount adjustment workflow.</summary>
public static class HeadcountAdjustmentWorkflow
{
    public static HeadcountAdjustmentDesk NewHeadcountAdjustmentDesk(IHeadcountAdjustmentStore store, IClock clock, Registry flags)
    {
        return new HeadcountAdjustmentDesk(
            store,
            HeadcountAdjustmentRules.DefaultHeadcountAdjustmentValidator(),
            clock,
            new List<string>(),
            HeadcountAdjustmentEngine.DefaultHeadcountAdjustmentEngine(flags));
    }
}
