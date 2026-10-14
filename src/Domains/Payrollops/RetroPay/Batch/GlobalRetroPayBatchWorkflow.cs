namespace Acme.Workforce.Payrollops.RetroPay.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves global retro pay batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record GlobalRetroPayBatchWorkflow(IGlobalRetroPayBatchStore Store, GlobalRetroPayBatchValidator Validator, IClock Clock, List<string> Journal)
{
    public static GlobalRetroPayBatchWorkflow NewGlobalRetroPayBatchWorkflow(IGlobalRetroPayBatchStore store, IClock clock)
    {
        return new GlobalRetroPayBatchWorkflow(
            store,
            GlobalRetroPayBatchRules.DefaultGlobalRetroPayBatchValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global retro pay batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalRetroPayBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global retro pay batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalRetroPayBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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
