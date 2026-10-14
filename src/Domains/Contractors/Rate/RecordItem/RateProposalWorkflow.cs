namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves rate proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record RateProposalWorkflow(IRateProposalStore Store, RateProposalValidator Validator, IClock Clock, List<string> Journal, RateProposalEngine Engine)
{
    public static RateProposalWorkflow NewRateProposalWorkflow(IRateProposalStore store, IClock clock)
    {
        return new RateProposalWorkflow(
            store,
            RateProposalRules.DefaultRateProposalValidator(),
            clock,
            new List<string>(),
            RateProposalEngine.DefaultRateProposalEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RateProposalStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RateProposalStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"rate proposal {code} not found");
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
