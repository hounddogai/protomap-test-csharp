namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback recognition preferences through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackRecognitionPreferenceWorkflow(IFallbackRecognitionPreferenceStore Store, FallbackRecognitionPreferenceValidator Validator, IClock Clock, List<string> Journal, FallbackRecognitionPreferenceEngine Engine)
{
    public static FallbackRecognitionPreferenceWorkflow NewFallbackRecognitionPreferenceWorkflow(IFallbackRecognitionPreferenceStore store, IClock clock)
    {
        return new FallbackRecognitionPreferenceWorkflow(
            store,
            FallbackRecognitionPreferenceRules.DefaultFallbackRecognitionPreferenceValidator(),
            clock,
            new List<string>(),
            FallbackRecognitionPreferenceEngine.DefaultFallbackRecognitionPreferenceEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback recognition preference {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackRecognitionPreferenceStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback recognition preference {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackRecognitionPreferenceStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback recognition preference {code} not found");
        }
        var result = Engine.Compute(item);
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
            if (item.StartsAt > cutoff)
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
