namespace Acme.Workforce.Engagement.Recognition.Milestone;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition milestones through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionMilestoneWorkflow(IRecognitionMilestoneStore Store, RecognitionMilestoneValidator Validator, IClock Clock, List<string> Journal, RecognitionMilestoneEngine Engine)
{
    public static RecognitionMilestoneWorkflow NewRecognitionMilestoneWorkflow(IRecognitionMilestoneStore store, IClock clock)
    {
        return new RecognitionMilestoneWorkflow(
            store,
            RecognitionMilestoneRules.DefaultRecognitionMilestoneValidator(),
            clock,
            new List<string>(),
            RecognitionMilestoneEngine.DefaultRecognitionMilestoneEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition milestone {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionMilestoneStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition milestone {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionMilestoneStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition milestone {code} not found");
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
