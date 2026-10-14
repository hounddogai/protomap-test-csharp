namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves bonus pool proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record BonusPoolProposalDesk(IBonusPoolProposalStore Store, BonusPoolProposalValidator Validator, IClock Clock, List<string> Journal, BonusPoolProposalEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(BonusPoolProposalStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(BonusPoolProposalStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"bonus pool proposal {code} not found");
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

/// <summary>Functions of bonus pool proposal workflow.</summary>
public static class BonusPoolProposalWorkflow
{
    public static BonusPoolProposalDesk NewBonusPoolProposalDesk(IBonusPoolProposalStore store, IClock clock, Registry flags)
    {
        return new BonusPoolProposalDesk(
            store,
            BonusPoolProposalRules.DefaultBonusPoolProposalValidator(),
            clock,
            new List<string>(),
            BonusPoolProposalEngine.DefaultBonusPoolProposalEngine(flags));
    }
}
