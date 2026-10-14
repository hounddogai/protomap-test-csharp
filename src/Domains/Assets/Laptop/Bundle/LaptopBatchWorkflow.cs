namespace Acme.Workforce.Assets.Laptop.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves laptop batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record LaptopBatchService(ILaptopBatchStore Store, LaptopBatchValidator Validator, IClock Clock, List<string> Journal, LaptopBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LaptopBatchStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LaptopBatchStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"laptop batch {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of laptop batch workflow.</summary>
public static class LaptopBatchWorkflow
{
    public static LaptopBatchService NewLaptopBatchService(ILaptopBatchStore store, IClock clock)
    {
        return new LaptopBatchService(
            store,
            LaptopBatchRules.DefaultLaptopBatchValidator(),
            clock,
            new List<string>(),
            LaptopBatchEngine.DefaultLaptopBatchEngine());
    }
}
