namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves primary mobility targets through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryMobilityTargetCoordinator(IPrimaryMobilityTargetStore Store, PrimaryMobilityTargetValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility target {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryMobilityTargetStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility target {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryMobilityTargetStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of primary mobility target workflow.</summary>
public static class PrimaryMobilityTargetWorkflow
{
    public static PrimaryMobilityTargetCoordinator NewPrimaryMobilityTargetCoordinator(IPrimaryMobilityTargetStore store, IClock clock)
    {
        return new PrimaryMobilityTargetCoordinator(
            store,
            PrimaryMobilityTargetRules.DefaultPrimaryMobilityTargetValidator(),
            clock,
            new List<string>());
    }
}
