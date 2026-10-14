namespace Acme.Workforce.Orgdesign.Span.SpanDraft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default span claims through their life cycle and keeps a journal of what it did.</summary>
public sealed record DefaultSpanClaimDesk(IDefaultSpanClaimStore Store, DefaultSpanClaimValidator Validator, IClock Clock, List<string> Journal, DefaultSpanClaimEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span claim {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultSpanClaimStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span claim {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultSpanClaimStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default span claim {code} not found");
        }
        var result = Engine.Run(item);
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
            if (item.ExpiresAt > cutoff)
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

/// <summary>Functions of default span claim workflow.</summary>
public static class DefaultSpanClaimWorkflow
{
    public static DefaultSpanClaimDesk NewDefaultSpanClaimDesk(IDefaultSpanClaimStore store, IClock clock)
    {
        return new DefaultSpanClaimDesk(
            store,
            DefaultSpanClaimRules.DefaultDefaultSpanClaimValidator(),
            clock,
            new List<string>(),
            DefaultSpanClaimEngine.DefaultDefaultSpanClaimEngine());
    }
}
