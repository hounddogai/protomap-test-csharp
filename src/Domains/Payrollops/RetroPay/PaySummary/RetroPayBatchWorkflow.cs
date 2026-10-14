namespace Acme.Workforce.Payrollops.RetroPay.PaySummary;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves retro pay batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record RetroPayBatchService(IRetroPayBatchStore Store, RetroPayBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RetroPayBatchStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"retro pay batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RetroPayBatchStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of retro pay batch workflow.</summary>
public static class RetroPayBatchWorkflow
{
    public static RetroPayBatchService NewRetroPayBatchService(IRetroPayBatchStore store, IClock clock)
    {
        return new RetroPayBatchService(
            store,
            RetroPayBatchRules.DefaultRetroPayBatchValidator(),
            clock,
            new List<string>());
    }
}
