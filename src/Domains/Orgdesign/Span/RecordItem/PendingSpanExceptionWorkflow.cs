namespace Acme.Workforce.Orgdesign.Span.RecordItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pending span exceptions through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingSpanExceptionDesk(IPendingSpanExceptionStore Store, PendingSpanExceptionValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending span exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingSpanExceptionStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending span exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingSpanExceptionStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of pending span exception workflow.</summary>
public static class PendingSpanExceptionWorkflow
{
    public static PendingSpanExceptionDesk NewPendingSpanExceptionDesk(IPendingSpanExceptionStore store, IClock clock)
    {
        return new PendingSpanExceptionDesk(
            store,
            PendingSpanExceptionRules.DefaultPendingSpanExceptionValidator(),
            clock,
            new List<string>());
    }
}
