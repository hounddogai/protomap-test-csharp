namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending attestation rules through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingAttestationRuleWorkflow(IPendingAttestationRuleStore Store, PendingAttestationRuleValidator Validator, IClock Clock, List<string> Journal, PendingAttestationRuleEngine Engine)
{
    public static PendingAttestationRuleWorkflow NewPendingAttestationRuleWorkflow(IPendingAttestationRuleStore store, IClock clock, Registry flags)
    {
        return new PendingAttestationRuleWorkflow(
            store,
            PendingAttestationRuleRules.DefaultPendingAttestationRuleValidator(),
            clock,
            new List<string>(),
            PendingAttestationRuleEngine.DefaultPendingAttestationRuleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation rule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAttestationRuleStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation rule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAttestationRuleStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation rule {code} not found");
        }
        var result = Engine.Compute(item);
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
