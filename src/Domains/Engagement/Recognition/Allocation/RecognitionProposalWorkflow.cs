namespace Acme.Workforce.Engagement.Recognition.Allocation;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionProposalWorkflow(IRecognitionProposalStore Store, RecognitionProposalValidator Validator, IClock Clock, List<string> Journal)
{
    public static RecognitionProposalWorkflow NewRecognitionProposalWorkflow(IRecognitionProposalStore store, IClock clock)
    {
        return new RecognitionProposalWorkflow(
            store,
            RecognitionProposalRules.DefaultRecognitionProposalValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionProposalStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionProposalStatus.Cancelled);
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
            if (item.ExpiresAt > cutoff)
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
