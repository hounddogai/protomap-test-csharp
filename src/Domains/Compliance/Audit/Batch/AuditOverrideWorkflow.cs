namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves audit overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record AuditOverrideService(IAuditOverrideStore Store, AuditOverrideValidator Validator, IClock Clock, List<string> Journal, AuditOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AuditOverrideStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AuditOverrideStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"audit override {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of audit override workflow.</summary>
public static class AuditOverrideWorkflow
{
    public static AuditOverrideService NewAuditOverrideService(IAuditOverrideStore store, IClock clock)
    {
        return new AuditOverrideService(
            store,
            AuditOverrideRules.DefaultAuditOverrideValidator(),
            clock,
            new List<string>(),
            AuditOverrideEngine.DefaultAuditOverrideEngine());
    }
}
