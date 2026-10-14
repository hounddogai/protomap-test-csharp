namespace Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary reorg claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryReorgClaimCoordinator(IPrimaryReorgClaimStore Store, PrimaryReorgClaimValidator Validator, IClock Clock, List<string> Journal, PrimaryReorgClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary reorg claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryReorgClaimStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary reorg claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryReorgClaimStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary reorg claim {code} not found");
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of primary reorg claim workflow.</summary>
public static class PrimaryReorgClaimWorkflow
{
    public static PrimaryReorgClaimCoordinator NewPrimaryReorgClaimCoordinator(IPrimaryReorgClaimStore store, IClock clock)
    {
        return new PrimaryReorgClaimCoordinator(
            store,
            PrimaryReorgClaimRules.DefaultPrimaryReorgClaimValidator(),
            clock,
            new List<string>(),
            PrimaryReorgClaimEngine.DefaultPrimaryReorgClaimEngine());
    }
}
