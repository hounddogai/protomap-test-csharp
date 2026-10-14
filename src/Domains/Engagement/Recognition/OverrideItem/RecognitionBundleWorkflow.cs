namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition bundles through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionBundleDesk(IRecognitionBundleStore Store, RecognitionBundleValidator Validator, IClock Clock, List<string> Journal, RecognitionBundleEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition bundle {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionBundleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition bundle {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionBundleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition bundle {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.EndsAt > cutoff)
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

/// <summary>Functions of recognition bundle workflow.</summary>
public static class RecognitionBundleWorkflow
{
    public static RecognitionBundleDesk NewRecognitionBundleDesk(IRecognitionBundleStore store, IClock clock, Registry flags)
    {
        return new RecognitionBundleDesk(
            store,
            RecognitionBundleRules.DefaultRecognitionBundleValidator(),
            clock,
            new List<string>(),
            RecognitionBundleEngine.DefaultRecognitionBundleEngine(flags));
    }
}
