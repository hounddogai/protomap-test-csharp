namespace Acme.Workforce.Compensation.EquityGrant.GrantBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient equity grant batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientEquityGrantBatchCoordinator(ILenientEquityGrantBatchStore Store, LenientEquityGrantBatchValidator Validator, IClock Clock, List<string> Journal, LenientEquityGrantBatchEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient equity grant batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientEquityGrantBatchStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient equity grant batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientEquityGrantBatchStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient equity grant batch {code} not found");
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

/// <summary>Functions of lenient equity grant batch workflow.</summary>
public static class LenientEquityGrantBatchWorkflow
{
    public static LenientEquityGrantBatchCoordinator NewLenientEquityGrantBatchCoordinator(ILenientEquityGrantBatchStore store, IClock clock, Registry flags)
    {
        return new LenientEquityGrantBatchCoordinator(
            store,
            LenientEquityGrantBatchRules.DefaultLenientEquityGrantBatchValidator(),
            clock,
            new List<string>(),
            LenientEquityGrantBatchEngine.DefaultLenientEquityGrantBatchEngine(flags));
    }
}
