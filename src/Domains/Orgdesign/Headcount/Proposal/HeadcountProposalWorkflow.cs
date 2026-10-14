namespace Acme.Workforce.Orgdesign.Headcount.Proposal;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves headcount proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record HeadcountProposalService(IHeadcountProposalStore Store, HeadcountProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(HeadcountProposalStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"headcount proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(HeadcountProposalStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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

/// <summary>Functions of headcount proposal workflow.</summary>
public static class HeadcountProposalWorkflow
{
    public static HeadcountProposalService NewHeadcountProposalService(IHeadcountProposalStore store, IClock clock)
    {
        return new HeadcountProposalService(
            store,
            HeadcountProposalRules.DefaultHeadcountProposalValidator(),
            clock,
            new List<string>());
    }
}
