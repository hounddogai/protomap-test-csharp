namespace Acme.Workforce.Payrollops.RetroPay.Quota;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay adjustments through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayAdjustmentManager(IRetroPayAdjustmentStore Store, RetroPayAdjustmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayAdjustmentStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayAdjustmentStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of retro pay adjustment workflow.</summary>
public static class RetroPayAdjustmentWorkflow
{
    public static RetroPayAdjustmentManager NewRetroPayAdjustmentManager(IRetroPayAdjustmentStore store, IClock clock)
    {
        return new RetroPayAdjustmentManager(
            store,
            RetroPayAdjustmentRules.DefaultRetroPayAdjustmentValidator(),
            clock,
            new List<string>());
    }
}
