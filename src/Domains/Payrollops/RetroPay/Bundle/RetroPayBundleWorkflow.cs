namespace Acme.Workforce.Payrollops.RetroPay.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayBundleWorkflow(IRetroPayBundleStore Store, RetroPayBundleValidator Validator, IClock Clock, List<string> Journal, RetroPayBundleEngine Engine)
{
    public static RetroPayBundleWorkflow NewRetroPayBundleWorkflow(IRetroPayBundleStore store, IClock clock)
    {
        return new RetroPayBundleWorkflow(
            store,
            RetroPayBundleRules.DefaultRetroPayBundleValidator(),
            clock,
            new List<string>(),
            RetroPayBundleEngine.DefaultRetroPayBundleEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayBundleStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayBundleStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay bundle {code} not found");
        }
        var result = Engine.Evaluate(item);
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
