namespace Acme.Workforce.Compliance.Attestation.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending attestation cycles through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingAttestationCycleWorkflow(IPendingAttestationCycleStore Store, PendingAttestationCycleValidator Validator, IClock Clock, List<string> Journal, PendingAttestationCycleEngine Engine)
{
    public static PendingAttestationCycleWorkflow NewPendingAttestationCycleWorkflow(IPendingAttestationCycleStore store, IClock clock, Registry flags)
    {
        return new PendingAttestationCycleWorkflow(
            store,
            PendingAttestationCycleRules.DefaultPendingAttestationCycleValidator(),
            clock,
            new List<string>(),
            PendingAttestationCycleEngine.DefaultPendingAttestationCycleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation cycle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingAttestationCycleStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation cycle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingAttestationCycleStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending attestation cycle {code} not found");
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
