namespace Acme.Workforce.Engagement.Recognition.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionVarianceCoordinator(IRecognitionVarianceStore Store, RecognitionVarianceValidator Validator, IClock Clock, List<string> Journal, RecognitionVarianceEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionVarianceStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionVarianceStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition variance {code} not found");
        }
        var result = Engine.Run(item);
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

/// <summary>Functions of recognition variance workflow.</summary>
public static class RecognitionVarianceWorkflow
{
    public static RecognitionVarianceCoordinator NewRecognitionVarianceCoordinator(IRecognitionVarianceStore store, IClock clock, Registry flags)
    {
        return new RecognitionVarianceCoordinator(
            store,
            RecognitionVarianceRules.DefaultRecognitionVarianceValidator(),
            clock,
            new List<string>(),
            RecognitionVarianceEngine.DefaultRecognitionVarianceEngine(flags));
    }
}
