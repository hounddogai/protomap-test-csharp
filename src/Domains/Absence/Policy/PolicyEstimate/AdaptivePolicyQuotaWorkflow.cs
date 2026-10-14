namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive policy quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record AdaptivePolicyQuotaWorkflow(IAdaptivePolicyQuotaStore Store, AdaptivePolicyQuotaValidator Validator, IClock Clock, List<string> Journal, AdaptivePolicyQuotaEngine Engine)
{
    public static AdaptivePolicyQuotaWorkflow NewAdaptivePolicyQuotaWorkflow(IAdaptivePolicyQuotaStore store, IClock clock, Registry flags)
    {
        return new AdaptivePolicyQuotaWorkflow(
            store,
            AdaptivePolicyQuotaRules.DefaultAdaptivePolicyQuotaValidator(),
            clock,
            new List<string>(),
            AdaptivePolicyQuotaEngine.DefaultAdaptivePolicyQuotaEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptivePolicyQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptivePolicyQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive policy quota {code} not found");
        }
        var result = Engine.Run(item);
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
