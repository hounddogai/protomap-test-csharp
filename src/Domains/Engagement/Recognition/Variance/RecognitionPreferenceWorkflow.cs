namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition preferences through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionPreferenceManager(IRecognitionPreferenceStore Store, RecognitionPreferenceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionPreferenceStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionPreferenceStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of recognition preference workflow.</summary>
public static class RecognitionPreferenceWorkflow
{
    public static RecognitionPreferenceManager NewRecognitionPreferenceManager(IRecognitionPreferenceStore store, IClock clock)
    {
        return new RecognitionPreferenceManager(
            store,
            RecognitionPreferenceRules.DefaultRecognitionPreferenceValidator(),
            clock,
            new List<string>());
    }
}
