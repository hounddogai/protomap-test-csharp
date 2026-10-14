namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending desk rollups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingDeskRollupWorkflow(IPendingDeskRollupStore Store, PendingDeskRollupValidator Validator, IClock Clock, List<string> Journal, PendingDeskRollupEngine Engine)
{
    public static PendingDeskRollupWorkflow NewPendingDeskRollupWorkflow(IPendingDeskRollupStore store, IClock clock, Registry flags)
    {
        return new PendingDeskRollupWorkflow(
            store,
            PendingDeskRollupRules.DefaultPendingDeskRollupValidator(),
            clock,
            new List<string>(),
            PendingDeskRollupEngine.DefaultPendingDeskRollupEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending desk rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingDeskRollupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending desk rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingDeskRollupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending desk rollup {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.ExpiresAt > cutoff)
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
