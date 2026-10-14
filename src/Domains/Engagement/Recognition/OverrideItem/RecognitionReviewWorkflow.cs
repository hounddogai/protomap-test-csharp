namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition reviews through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionReviewDesk(IRecognitionReviewStore Store, RecognitionReviewValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition review {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionReviewStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition review {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionReviewStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of recognition review workflow.</summary>
public static class RecognitionReviewWorkflow
{
    public static RecognitionReviewDesk NewRecognitionReviewDesk(IRecognitionReviewStore store, IClock clock)
    {
        return new RecognitionReviewDesk(
            store,
            RecognitionReviewRules.DefaultRecognitionReviewValidator(),
            clock,
            new List<string>());
    }
}
