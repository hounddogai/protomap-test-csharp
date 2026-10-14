namespace Acme.Workforce.Compliance.TrainingDue.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves training due windows through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueWindowManager(ITrainingDueWindowStore Store, TrainingDueWindowValidator Validator, IClock Clock, List<string> Journal, TrainingDueWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueWindowStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueWindowStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due window {code} not found");
        }
        var result = Engine.Evaluate(item);
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

/// <summary>Functions of training due window workflow.</summary>
public static class TrainingDueWindowWorkflow
{
    public static TrainingDueWindowManager NewTrainingDueWindowManager(ITrainingDueWindowStore store, IClock clock, Registry flags)
    {
        return new TrainingDueWindowManager(
            store,
            TrainingDueWindowRules.DefaultTrainingDueWindowValidator(),
            clock,
            new List<string>(),
            TrainingDueWindowEngine.DefaultTrainingDueWindowEngine(flags));
    }
}
