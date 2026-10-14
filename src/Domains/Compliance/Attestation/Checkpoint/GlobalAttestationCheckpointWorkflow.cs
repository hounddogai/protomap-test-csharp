namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves global attestation checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record GlobalAttestationCheckpointService(IGlobalAttestationCheckpointStore Store, GlobalAttestationCheckpointValidator Validator, IClock Clock, List<string> Journal, GlobalAttestationCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global attestation checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(GlobalAttestationCheckpointStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global attestation checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(GlobalAttestationCheckpointStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"global attestation checkpoint {code} not found");
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

/// <summary>Functions of global attestation checkpoint workflow.</summary>
public static class GlobalAttestationCheckpointWorkflow
{
    public static GlobalAttestationCheckpointService NewGlobalAttestationCheckpointService(IGlobalAttestationCheckpointStore store, IClock clock, Registry flags)
    {
        return new GlobalAttestationCheckpointService(
            store,
            GlobalAttestationCheckpointRules.DefaultGlobalAttestationCheckpointValidator(),
            clock,
            new List<string>(),
            GlobalAttestationCheckpointEngine.DefaultGlobalAttestationCheckpointEngine(flags));
    }
}
