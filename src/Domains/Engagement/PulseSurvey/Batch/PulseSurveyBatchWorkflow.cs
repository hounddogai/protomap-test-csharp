namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves pulse survey batches through their life cycle and keeps a journal of what it did.</summary>
public sealed record PulseSurveyBatchCoordinator(IPulseSurveyBatchStore Store, PulseSurveyBatchValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PulseSurveyBatchStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pulse survey batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PulseSurveyBatchStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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

/// <summary>Functions of pulse survey batch workflow.</summary>
public static class PulseSurveyBatchWorkflow
{
    public static PulseSurveyBatchCoordinator NewPulseSurveyBatchCoordinator(IPulseSurveyBatchStore store, IClock clock)
    {
        return new PulseSurveyBatchCoordinator(
            store,
            PulseSurveyBatchRules.DefaultPulseSurveyBatchValidator(),
            clock,
            new List<string>());
    }
}
