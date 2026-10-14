namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves rolling policy overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record RollingPolicyOverrideDesk(IRollingPolicyOverrideStore Store, RollingPolicyOverrideValidator Validator, IClock Clock, List<string> Journal, RollingPolicyOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling policy override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RollingPolicyOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling policy override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RollingPolicyOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rolling policy override {code} not found");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of rolling policy override workflow.</summary>
public static class RollingPolicyOverrideWorkflow
{
    public static RollingPolicyOverrideDesk NewRollingPolicyOverrideDesk(IRollingPolicyOverrideStore store, IClock clock, Registry flags)
    {
        return new RollingPolicyOverrideDesk(
            store,
            RollingPolicyOverrideRules.DefaultRollingPolicyOverrideValidator(),
            clock,
            new List<string>(),
            RollingPolicyOverrideEngine.DefaultRollingPolicyOverrideEngine(flags));
    }
}
