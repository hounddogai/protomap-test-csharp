namespace Acme.Workforce.Compliance.Certification.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered certification checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredCertificationCheckpointManager(ITieredCertificationCheckpointStore Store, TieredCertificationCheckpointValidator Validator, IClock Clock, List<string> Journal, TieredCertificationCheckpointEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered certification checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredCertificationCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered certification checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredCertificationCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered certification checkpoint {code} not found");
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

/// <summary>Functions of tiered certification checkpoint workflow.</summary>
public static class TieredCertificationCheckpointWorkflow
{
    public static TieredCertificationCheckpointManager NewTieredCertificationCheckpointManager(ITieredCertificationCheckpointStore store, IClock clock)
    {
        return new TieredCertificationCheckpointManager(
            store,
            TieredCertificationCheckpointRules.DefaultTieredCertificationCheckpointValidator(),
            clock,
            new List<string>(),
            TieredCertificationCheckpointEngine.DefaultTieredCertificationCheckpointEngine());
    }
}
