namespace Acme.Workforce.Payrollops.RetroPay.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves retro pay notices through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayNoticeCoordinator(IRetroPayNoticeStore Store, RetroPayNoticeValidator Validator, IClock Clock, List<string> Journal, RetroPayNoticeEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay notice {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayNoticeStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay notice {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayNoticeStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay notice {code} not found");
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

/// <summary>Functions of retro pay notice workflow.</summary>
public static class RetroPayNoticeWorkflow
{
    public static RetroPayNoticeCoordinator NewRetroPayNoticeCoordinator(IRetroPayNoticeStore store, IClock clock, Registry flags)
    {
        return new RetroPayNoticeCoordinator(
            store,
            RetroPayNoticeRules.DefaultRetroPayNoticeValidator(),
            clock,
            new List<string>(),
            RetroPayNoticeEngine.DefaultRetroPayNoticeEngine(flags));
    }
}
