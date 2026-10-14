namespace Acme.Workforce.Payrollops.RetroPay.PayBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves retro pay targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayTargetManager(IRetroPayTargetStore Store, RetroPayTargetValidator Validator, IClock Clock, List<string> Journal, RetroPayTargetEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayTargetStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayTargetStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay target {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of retro pay target workflow.</summary>
public static class RetroPayTargetWorkflow
{
    public static RetroPayTargetManager NewRetroPayTargetManager(IRetroPayTargetStore store, IClock clock, Registry flags)
    {
        return new RetroPayTargetManager(
            store,
            RetroPayTargetRules.DefaultRetroPayTargetValidator(),
            clock,
            new List<string>(),
            RetroPayTargetEngine.DefaultRetroPayTargetEngine(flags));
    }
}
