namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves mobility baselines through their life cycle and keeps a journal of what it did.</summary>
public sealed record MobilityBaselineService(IMobilityBaselineStore Store, MobilityBaselineValidator Validator, IClock Clock, List<string> Journal, MobilityBaselineEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility baseline {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MobilityBaselineStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility baseline {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MobilityBaselineStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"mobility baseline {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of mobility baseline workflow.</summary>
public static class MobilityBaselineWorkflow
{
    public static MobilityBaselineService NewMobilityBaselineService(IMobilityBaselineStore store, IClock clock)
    {
        return new MobilityBaselineService(
            store,
            MobilityBaselineRules.DefaultMobilityBaselineValidator(),
            clock,
            new List<string>(),
            MobilityBaselineEngine.DefaultMobilityBaselineEngine());
    }
}
