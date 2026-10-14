namespace Acme.Workforce.Orgdesign.Span.Slot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves span overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record SpanOverrideWorkflow(ISpanOverrideStore Store, SpanOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    public static SpanOverrideWorkflow NewSpanOverrideWorkflow(ISpanOverrideStore store, IClock clock)
    {
        return new SpanOverrideWorkflow(
            store,
            SpanOverrideRules.DefaultSpanOverrideValidator(),
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
            throw new InvalidOperationException($"span override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SpanOverrideStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"span override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SpanOverrideStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
