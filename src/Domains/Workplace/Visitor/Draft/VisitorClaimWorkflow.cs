namespace Acme.Workforce.Workplace.Visitor.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves visitor claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record VisitorClaimWorkflow(IVisitorClaimStore Store, VisitorClaimValidator Validator, IClock Clock, List<string> Journal, VisitorClaimEngine Engine)
{
    public static VisitorClaimWorkflow NewVisitorClaimWorkflow(IVisitorClaimStore store, IClock clock)
    {
        return new VisitorClaimWorkflow(
            store,
            VisitorClaimRules.DefaultVisitorClaimValidator(),
            clock,
            new List<string>(),
            VisitorClaimEngine.DefaultVisitorClaimEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(VisitorClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(VisitorClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"visitor claim {code} not found");
        }
        var result = Engine.Price(item);
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
