namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves desk claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record DeskClaimService(IDeskClaimStore Store, DeskClaimValidator Validator, IClock Clock, List<string> Journal, DeskClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DeskClaimStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DeskClaimStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"desk claim {code} not found");
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

/// <summary>Functions of desk claim workflow.</summary>
public static class DeskClaimWorkflow
{
    public static DeskClaimService NewDeskClaimService(IDeskClaimStore store, IClock clock, Registry flags)
    {
        return new DeskClaimService(
            store,
            DeskClaimRules.DefaultDeskClaimValidator(),
            clock,
            new List<string>(),
            DeskClaimEngine.DefaultDeskClaimEngine(flags));
    }
}
