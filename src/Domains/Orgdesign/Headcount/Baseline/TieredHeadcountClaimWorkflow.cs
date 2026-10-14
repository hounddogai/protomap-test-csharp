namespace Acme.Workforce.Orgdesign.Headcount.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered headcount claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredHeadcountClaimWorkflow(ITieredHeadcountClaimStore Store, TieredHeadcountClaimValidator Validator, IClock Clock, List<string> Journal)
{
    public static TieredHeadcountClaimWorkflow NewTieredHeadcountClaimWorkflow(ITieredHeadcountClaimStore store, IClock clock)
    {
        return new TieredHeadcountClaimWorkflow(
            store,
            TieredHeadcountClaimRules.DefaultTieredHeadcountClaimValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered headcount claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredHeadcountClaimStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered headcount claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredHeadcountClaimStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
