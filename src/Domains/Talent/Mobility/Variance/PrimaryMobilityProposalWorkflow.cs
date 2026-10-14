namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary mobility proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryMobilityProposalManager(IPrimaryMobilityProposalStore Store, PrimaryMobilityProposalValidator Validator, IClock Clock, List<string> Journal, PrimaryMobilityProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryMobilityProposalStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryMobilityProposalStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary mobility proposal {code} not found");
        }
        var result = Engine.Run(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of primary mobility proposal workflow.</summary>
public static class PrimaryMobilityProposalWorkflow
{
    public static PrimaryMobilityProposalManager NewPrimaryMobilityProposalManager(IPrimaryMobilityProposalStore store, IClock clock, Registry flags)
    {
        return new PrimaryMobilityProposalManager(
            store,
            PrimaryMobilityProposalRules.DefaultPrimaryMobilityProposalValidator(),
            clock,
            new List<string>(),
            PrimaryMobilityProposalEngine.DefaultPrimaryMobilityProposalEngine(flags));
    }
}
