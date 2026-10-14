namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition assignments through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionAssignmentCoordinator(IRecognitionAssignmentStore Store, RecognitionAssignmentValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition assignment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionAssignmentStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition assignment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionAssignmentStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of recognition assignment workflow.</summary>
public static class RecognitionAssignmentWorkflow
{
    public static RecognitionAssignmentCoordinator NewRecognitionAssignmentCoordinator(IRecognitionAssignmentStore store, IClock clock)
    {
        return new RecognitionAssignmentCoordinator(
            store,
            RecognitionAssignmentRules.DefaultRecognitionAssignmentValidator(),
            clock,
            new List<string>());
    }
}
