namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition overrides through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionOverrideDesk(IRecognitionOverrideStore Store, RecognitionOverrideValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition override {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionOverrideStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition override {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionOverrideStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of recognition override workflow.</summary>
public static class RecognitionOverrideWorkflow
{
    public static RecognitionOverrideDesk NewRecognitionOverrideDesk(IRecognitionOverrideStore store, IClock clock)
    {
        return new RecognitionOverrideDesk(
            store,
            RecognitionOverrideRules.DefaultRecognitionOverrideValidator(),
            clock,
            new List<string>());
    }
}
