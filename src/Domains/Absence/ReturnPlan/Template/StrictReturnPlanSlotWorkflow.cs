namespace Acme.Workforce.Absence.ReturnPlan.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict return plan slots through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictReturnPlanSlotManager(IStrictReturnPlanSlotStore Store, StrictReturnPlanSlotValidator Validator, IClock Clock, List<string> Journal, StrictReturnPlanSlotEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan slot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictReturnPlanSlotStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan slot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictReturnPlanSlotStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict return plan slot {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of strict return plan slot workflow.</summary>
public static class StrictReturnPlanSlotWorkflow
{
    public static StrictReturnPlanSlotManager NewStrictReturnPlanSlotManager(IStrictReturnPlanSlotStore store, IClock clock)
    {
        return new StrictReturnPlanSlotManager(
            store,
            StrictReturnPlanSlotRules.DefaultStrictReturnPlanSlotValidator(),
            clock,
            new List<string>(),
            StrictReturnPlanSlotEngine.DefaultStrictReturnPlanSlotEngine());
    }
}
