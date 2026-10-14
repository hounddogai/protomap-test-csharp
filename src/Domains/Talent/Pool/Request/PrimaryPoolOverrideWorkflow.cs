namespace Acme.Workforce.Talent.Pool.Request;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary pool overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPoolOverrideCoordinator(IPrimaryPoolOverrideStore Store, PrimaryPoolOverrideValidator Validator, IClock Clock, List<string> Journal, PrimaryPoolOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pool override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPoolOverrideStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pool override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPoolOverrideStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary pool override {code} not found");
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

/// <summary>Functions of primary pool override workflow.</summary>
public static class PrimaryPoolOverrideWorkflow
{
    public static PrimaryPoolOverrideCoordinator NewPrimaryPoolOverrideCoordinator(IPrimaryPoolOverrideStore store, IClock clock)
    {
        return new PrimaryPoolOverrideCoordinator(
            store,
            PrimaryPoolOverrideRules.DefaultPrimaryPoolOverrideValidator(),
            clock,
            new List<string>(),
            PrimaryPoolOverrideEngine.DefaultPrimaryPoolOverrideEngine());
    }
}
