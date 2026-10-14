namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient balance batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientBalanceBatchWorkflow(ILenientBalanceBatchStore Store, LenientBalanceBatchValidator Validator, IClock Clock, List<string> Journal, LenientBalanceBatchEngine Engine)
{
    public static LenientBalanceBatchWorkflow NewLenientBalanceBatchWorkflow(ILenientBalanceBatchStore store, IClock clock, Registry flags)
    {
        return new LenientBalanceBatchWorkflow(
            store,
            LenientBalanceBatchRules.DefaultLenientBalanceBatchValidator(),
            clock,
            new List<string>(),
            LenientBalanceBatchEngine.DefaultLenientBalanceBatchEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBalanceBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBalanceBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance batch {code} not found");
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
            if (item.EndsAt > cutoff)
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
