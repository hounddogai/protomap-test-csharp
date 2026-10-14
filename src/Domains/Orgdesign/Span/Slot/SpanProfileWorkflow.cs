namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanProfileWorkflow(ISpanProfileStore Store, SpanProfileValidator Validator, IClock Clock, List<string> Journal, SpanProfileEngine Engine)
{
    public static SpanProfileWorkflow NewSpanProfileWorkflow(ISpanProfileStore store, IClock clock)
    {
        return new SpanProfileWorkflow(
            store,
            SpanProfileRules.DefaultSpanProfileValidator(),
            clock,
            new List<string>(),
            SpanProfileEngine.DefaultSpanProfileEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanProfileStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanProfileStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span profile {code} not found");
        }
        var result = Engine.Price(item);
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
