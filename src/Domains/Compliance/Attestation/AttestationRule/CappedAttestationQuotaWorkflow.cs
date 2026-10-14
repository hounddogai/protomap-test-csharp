namespace Acme.Workforce.Compliance.Attestation.AttestationRule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves capped attestation quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record CappedAttestationQuotaWorkflow(ICappedAttestationQuotaStore Store, CappedAttestationQuotaValidator Validator, IClock Clock, List<string> Journal, CappedAttestationQuotaEngine Engine)
{
    public static CappedAttestationQuotaWorkflow NewCappedAttestationQuotaWorkflow(ICappedAttestationQuotaStore store, IClock clock, Registry flags)
    {
        return new CappedAttestationQuotaWorkflow(
            store,
            CappedAttestationQuotaRules.DefaultCappedAttestationQuotaValidator(),
            clock,
            new List<string>(),
            CappedAttestationQuotaEngine.DefaultCappedAttestationQuotaEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(CappedAttestationQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(CappedAttestationQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"capped attestation quota {code} not found");
        }
        var result = Engine.Price(item);
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
            if (item.UpdatedAt > cutoff)
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
