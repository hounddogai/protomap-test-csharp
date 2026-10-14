namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves merit cycle proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record MeritCycleProposalDesk(IMeritCycleProposalStore Store, MeritCycleProposalValidator Validator, IClock Clock, List<string> Journal, MeritCycleProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(MeritCycleProposalStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(MeritCycleProposalStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"merit cycle proposal {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of merit cycle proposal workflow.</summary>
public static class MeritCycleProposalWorkflow
{
    public static MeritCycleProposalDesk NewMeritCycleProposalDesk(IMeritCycleProposalStore store, IClock clock, Registry flags)
    {
        return new MeritCycleProposalDesk(
            store,
            MeritCycleProposalRules.DefaultMeritCycleProposalValidator(),
            clock,
            new List<string>(),
            MeritCycleProposalEngine.DefaultMeritCycleProposalEngine(flags));
    }
}
