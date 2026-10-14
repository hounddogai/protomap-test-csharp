namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves visitor adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorAdjustmentDesk(IVisitorAdjustmentStore Store, VisitorAdjustmentValidator Validator, IClock Clock, List<string> Journal, VisitorAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorAdjustmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorAdjustmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor adjustment {code} not found");
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

/// <summary>Functions of visitor adjustment workflow.</summary>
public static class VisitorAdjustmentWorkflow
{
    public static VisitorAdjustmentDesk NewVisitorAdjustmentDesk(IVisitorAdjustmentStore store, IClock clock, Registry flags)
    {
        return new VisitorAdjustmentDesk(
            store,
            VisitorAdjustmentRules.DefaultVisitorAdjustmentValidator(),
            clock,
            new List<string>(),
            VisitorAdjustmentEngine.DefaultVisitorAdjustmentEngine(flags));
    }
}
