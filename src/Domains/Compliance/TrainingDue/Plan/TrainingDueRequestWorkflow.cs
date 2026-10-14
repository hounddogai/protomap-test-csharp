namespace Acme.Workforce.Compliance.TrainingDue.Plan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due requests through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDueRequestDesk(ITrainingDueRequestStore Store, TrainingDueRequestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due request {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDueRequestStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due request {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDueRequestStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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

/// <summary>Functions of training due request workflow.</summary>
public static class TrainingDueRequestWorkflow
{
    public static TrainingDueRequestDesk NewTrainingDueRequestDesk(ITrainingDueRequestStore store, IClock clock)
    {
        return new TrainingDueRequestDesk(
            store,
            TrainingDueRequestRules.DefaultTrainingDueRequestValidator(),
            clock,
            new List<string>());
    }
}
