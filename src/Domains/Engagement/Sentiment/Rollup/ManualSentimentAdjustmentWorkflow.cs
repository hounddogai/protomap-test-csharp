namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual sentiment adjustments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record ManualSentimentAdjustmentService(IManualSentimentAdjustmentStore Store, ManualSentimentAdjustmentValidator Validator, IClock Clock, List<string> Journal, ManualSentimentAdjustmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual sentiment adjustment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualSentimentAdjustmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual sentiment adjustment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualSentimentAdjustmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual sentiment adjustment {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of manual sentiment adjustment workflow.</summary>
public static class ManualSentimentAdjustmentWorkflow
{
    public static ManualSentimentAdjustmentService NewManualSentimentAdjustmentService(IManualSentimentAdjustmentStore store, IClock clock)
    {
        return new ManualSentimentAdjustmentService(
            store,
            ManualSentimentAdjustmentRules.DefaultManualSentimentAdjustmentValidator(),
            clock,
            new List<string>(),
            ManualSentimentAdjustmentEngine.DefaultManualSentimentAdjustmentEngine());
    }
}
