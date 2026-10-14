namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves default mobility claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultMobilityClaimManager(IDefaultMobilityClaimStore Store, DefaultMobilityClaimValidator Validator, IClock Clock, List<string> Journal, DefaultMobilityClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultMobilityClaimStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultMobilityClaimStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default mobility claim {code} not found");
        }
        var result = Engine.Compute(item);
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

/// <summary>Functions of default mobility claim workflow.</summary>
public static class DefaultMobilityClaimWorkflow
{
    public static DefaultMobilityClaimManager NewDefaultMobilityClaimManager(IDefaultMobilityClaimStore store, IClock clock, Registry flags)
    {
        return new DefaultMobilityClaimManager(
            store,
            DefaultMobilityClaimRules.DefaultDefaultMobilityClaimValidator(),
            clock,
            new List<string>(),
            DefaultMobilityClaimEngine.DefaultDefaultMobilityClaimEngine(flags));
    }
}
