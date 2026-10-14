namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped balance quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedBalanceQuotaWorkflow(ICappedBalanceQuotaStore Store, CappedBalanceQuotaValidator Validator, IClock Clock, List<string> Journal, CappedBalanceQuotaEngine Engine)
{
    public static CappedBalanceQuotaWorkflow NewCappedBalanceQuotaWorkflow(ICappedBalanceQuotaStore store, IClock clock, Registry flags)
    {
        return new CappedBalanceQuotaWorkflow(
            store,
            CappedBalanceQuotaRules.DefaultCappedBalanceQuotaValidator(),
            clock,
            new List<string>(),
            CappedBalanceQuotaEngine.DefaultCappedBalanceQuotaEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped balance quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedBalanceQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped balance quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedBalanceQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped balance quota {code} not found");
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
