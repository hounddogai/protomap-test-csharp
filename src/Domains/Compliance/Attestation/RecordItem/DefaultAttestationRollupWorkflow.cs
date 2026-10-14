namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default attestation rollups through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultAttestationRollupCoordinator(IDefaultAttestationRollupStore Store, DefaultAttestationRollupValidator Validator, IClock Clock, List<string> Journal, DefaultAttestationRollupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default attestation rollup {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultAttestationRollupStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default attestation rollup {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultAttestationRollupStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default attestation rollup {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of default attestation rollup workflow.</summary>
public static class DefaultAttestationRollupWorkflow
{
    public static DefaultAttestationRollupCoordinator NewDefaultAttestationRollupCoordinator(IDefaultAttestationRollupStore store, IClock clock, Registry flags)
    {
        return new DefaultAttestationRollupCoordinator(
            store,
            DefaultAttestationRollupRules.DefaultDefaultAttestationRollupValidator(),
            clock,
            new List<string>(),
            DefaultAttestationRollupEngine.DefaultDefaultAttestationRollupEngine(flags));
    }
}
