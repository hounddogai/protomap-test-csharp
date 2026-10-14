namespace Acme.Workforce.Orgdesign.Span.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanQuotaWorkflow(ISpanQuotaStore Store, SpanQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    public static SpanQuotaWorkflow NewSpanQuotaWorkflow(ISpanQuotaStore store, IClock clock)
    {
        return new SpanQuotaWorkflow(store, SpanQuotaRules.DefaultSpanQuotaValidator(), clock, new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanQuotaStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanQuotaStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
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
