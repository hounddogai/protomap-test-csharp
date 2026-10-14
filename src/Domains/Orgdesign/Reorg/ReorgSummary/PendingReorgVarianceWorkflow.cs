namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending reorg variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingReorgVarianceWorkflow(IPendingReorgVarianceStore Store, PendingReorgVarianceValidator Validator, IClock Clock, List<string> Journal, PendingReorgVarianceEngine Engine)
{
    public static PendingReorgVarianceWorkflow NewPendingReorgVarianceWorkflow(IPendingReorgVarianceStore store, IClock clock, Registry flags)
    {
        return new PendingReorgVarianceWorkflow(
            store,
            PendingReorgVarianceRules.DefaultPendingReorgVarianceValidator(),
            clock,
            new List<string>(),
            PendingReorgVarianceEngine.DefaultPendingReorgVarianceEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingReorgVarianceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingReorgVarianceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending reorg variance {code} not found");
        }
        var result = Engine.Compute(item);
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
