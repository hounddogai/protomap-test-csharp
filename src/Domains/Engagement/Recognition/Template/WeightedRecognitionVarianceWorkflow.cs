namespace Acme.Workforce.Engagement.Recognition.Template;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves weighted recognition variances through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record WeightedRecognitionVarianceService(IWeightedRecognitionVarianceStore Store, WeightedRecognitionVarianceValidator Validator, IClock Clock, List<string> Journal, WeightedRecognitionVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted recognition variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(WeightedRecognitionVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted recognition variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(WeightedRecognitionVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"weighted recognition variance {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of weighted recognition variance workflow.</summary>
public static class WeightedRecognitionVarianceWorkflow
{
    public static WeightedRecognitionVarianceService NewWeightedRecognitionVarianceService(IWeightedRecognitionVarianceStore store, IClock clock)
    {
        return new WeightedRecognitionVarianceService(
            store,
            WeightedRecognitionVarianceRules.DefaultWeightedRecognitionVarianceValidator(),
            clock,
            new List<string>(),
            WeightedRecognitionVarianceEngine.DefaultWeightedRecognitionVarianceEngine());
    }
}
