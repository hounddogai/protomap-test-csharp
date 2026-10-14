namespace Acme.Workforce.Absence.Policy.PolicyRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive policy overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePolicyOverrideDesk(IAdaptivePolicyOverrideStore Store, AdaptivePolicyOverrideValidator Validator, IClock Clock, List<string> Journal, AdaptivePolicyOverrideEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePolicyOverrideStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePolicyOverrideStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy override {code} not found");
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

/// <summary>Functions of adaptive policy override workflow.</summary>
public static class AdaptivePolicyOverrideWorkflow
{
    public static AdaptivePolicyOverrideDesk NewAdaptivePolicyOverrideDesk(IAdaptivePolicyOverrideStore store, IClock clock, Registry flags)
    {
        return new AdaptivePolicyOverrideDesk(
            store,
            AdaptivePolicyOverrideRules.DefaultAdaptivePolicyOverrideValidator(),
            clock,
            new List<string>(),
            AdaptivePolicyOverrideEngine.DefaultAdaptivePolicyOverrideEngine(flags));
    }
}
