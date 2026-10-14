namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pool records through their life cycle and keeps a journal of what it did.</summary>
public sealed record PoolRecordService(IPoolRecordStore Store, PoolRecordValidator Validator, IClock Clock, List<string> Journal, PoolRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PoolRecordStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PoolRecordStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pool record {code} not found");
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

/// <summary>Functions of pool record workflow.</summary>
public static class PoolRecordWorkflow
{
    public static PoolRecordService NewPoolRecordService(IPoolRecordStore store, IClock clock)
    {
        return new PoolRecordService(
            store,
            PoolRecordRules.DefaultPoolRecordValidator(),
            clock,
            new List<string>(),
            PoolRecordEngine.DefaultPoolRecordEngine());
    }
}
