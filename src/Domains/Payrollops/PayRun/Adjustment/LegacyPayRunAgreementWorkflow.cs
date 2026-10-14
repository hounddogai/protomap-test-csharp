namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves legacy pay run agreements through their life cycle and keeps a journal of what it did.</summary>
public sealed record LegacyPayRunAgreementService(ILegacyPayRunAgreementStore Store, LegacyPayRunAgreementValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy pay run agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyPayRunAgreementStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy pay run agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyPayRunAgreementStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of legacy pay run agreement workflow.</summary>
public static class LegacyPayRunAgreementWorkflow
{
    public static LegacyPayRunAgreementService NewLegacyPayRunAgreementService(ILegacyPayRunAgreementStore store, IClock clock)
    {
        return new LegacyPayRunAgreementService(
            store,
            LegacyPayRunAgreementRules.DefaultLegacyPayRunAgreementValidator(),
            clock,
            new List<string>());
    }
}
