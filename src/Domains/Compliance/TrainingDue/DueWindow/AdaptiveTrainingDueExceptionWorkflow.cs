namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves adaptive training due exceptions through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record AdaptiveTrainingDueExceptionService(IAdaptiveTrainingDueExceptionStore Store, AdaptiveTrainingDueExceptionValidator Validator, IClock Clock, List<string> Journal, AdaptiveTrainingDueExceptionEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive training due exception {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(AdaptiveTrainingDueExceptionStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive training due exception {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(AdaptiveTrainingDueExceptionStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"adaptive training due exception {code} not found");
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
            if (item.CreatedAt > cutoff)
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

/// <summary>Functions of adaptive training due exception workflow.</summary>
public static class AdaptiveTrainingDueExceptionWorkflow
{
    public static AdaptiveTrainingDueExceptionService NewAdaptiveTrainingDueExceptionService(IAdaptiveTrainingDueExceptionStore store, IClock clock, Registry flags)
    {
        return new AdaptiveTrainingDueExceptionService(
            store,
            AdaptiveTrainingDueExceptionRules.DefaultAdaptiveTrainingDueExceptionValidator(),
            clock,
            new List<string>(),
            AdaptiveTrainingDueExceptionEngine.DefaultAdaptiveTrainingDueExceptionEngine(flags));
    }
}
