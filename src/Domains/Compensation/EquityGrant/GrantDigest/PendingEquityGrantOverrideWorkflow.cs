namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending equity grant overrides through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PendingEquityGrantOverrideDesk(IPendingEquityGrantOverrideStore Store, PendingEquityGrantOverrideValidator Validator, IClock Clock, List<string> Journal, PendingEquityGrantOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingEquityGrantOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingEquityGrantOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending equity grant override {code} not found");
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

/// <summary>Functions of pending equity grant override workflow.</summary>
public static class PendingEquityGrantOverrideWorkflow
{
    public static PendingEquityGrantOverrideDesk NewPendingEquityGrantOverrideDesk(IPendingEquityGrantOverrideStore store, IClock clock, Registry flags)
    {
        return new PendingEquityGrantOverrideDesk(
            store,
            PendingEquityGrantOverrideRules.DefaultPendingEquityGrantOverrideValidator(),
            clock,
            new List<string>(),
            PendingEquityGrantOverrideEngine.DefaultPendingEquityGrantOverrideEngine(flags));
    }
}
