namespace Acme.Workforce.Talent.Mobility.Bundle;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility manifests through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityManifestWorkflow(IMobilityManifestStore Store, MobilityManifestValidator Validator, IClock Clock, List<string> Journal, MobilityManifestEngine Engine)
{
    public static MobilityManifestWorkflow NewMobilityManifestWorkflow(IMobilityManifestStore store, IClock clock)
    {
        return new MobilityManifestWorkflow(
            store,
            MobilityManifestRules.DefaultMobilityManifestValidator(),
            clock,
            new List<string>(),
            MobilityManifestEngine.DefaultMobilityManifestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility manifest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityManifestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility manifest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityManifestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility manifest {code} not found");
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
