namespace Acme.Workforce.Payrollops.RetroPay.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves batched retro pay policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record BatchedRetroPayPolicyManager(IBatchedRetroPayPolicyStore Store, BatchedRetroPayPolicyValidator Validator, IClock Clock, List<string> Journal, BatchedRetroPayPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched retro pay policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BatchedRetroPayPolicyStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched retro pay policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BatchedRetroPayPolicyStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"batched retro pay policy {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of batched retro pay policy workflow.</summary>
public static class BatchedRetroPayPolicyWorkflow
{
    public static BatchedRetroPayPolicyManager NewBatchedRetroPayPolicyManager(IBatchedRetroPayPolicyStore store, IClock clock, Registry flags)
    {
        return new BatchedRetroPayPolicyManager(
            store,
            BatchedRetroPayPolicyRules.DefaultBatchedRetroPayPolicyValidator(),
            clock,
            new List<string>(),
            BatchedRetroPayPolicyEngine.DefaultBatchedRetroPayPolicyEngine(flags));
    }
}
