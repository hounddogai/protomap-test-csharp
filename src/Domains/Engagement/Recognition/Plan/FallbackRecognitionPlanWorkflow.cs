namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback recognition plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackRecognitionPlanService(IFallbackRecognitionPlanStore Store, FallbackRecognitionPlanValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback recognition plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackRecognitionPlanStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback recognition plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackRecognitionPlanStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
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

/// <summary>Functions of fallback recognition plan workflow.</summary>
public static class FallbackRecognitionPlanWorkflow
{
    public static FallbackRecognitionPlanService NewFallbackRecognitionPlanService(IFallbackRecognitionPlanStore store, IClock clock)
    {
        return new FallbackRecognitionPlanService(
            store,
            FallbackRecognitionPlanRules.DefaultFallbackRecognitionPlanValidator(),
            clock,
            new List<string>());
    }
}
