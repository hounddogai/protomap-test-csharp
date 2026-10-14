namespace Acme.Workforce.Orgdesign.Span.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending span claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingSpanClaimWorkflow(IPendingSpanClaimStore Store, PendingSpanClaimValidator Validator, IClock Clock, List<string> Journal, PendingSpanClaimEngine Engine)
{
    public static PendingSpanClaimWorkflow NewPendingSpanClaimWorkflow(IPendingSpanClaimStore store, IClock clock, Registry flags)
    {
        return new PendingSpanClaimWorkflow(
            store,
            PendingSpanClaimRules.DefaultPendingSpanClaimValidator(),
            clock,
            new List<string>(),
            PendingSpanClaimEngine.DefaultPendingSpanClaimEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending span claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSpanClaimStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending span claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSpanClaimStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending span claim {code} not found");
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
