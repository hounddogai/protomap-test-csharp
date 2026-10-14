namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves recognition digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionDigestManager(IRecognitionDigestStore Store, RecognitionDigestValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionDigestStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionDigestStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
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

/// <summary>Functions of recognition digest workflow.</summary>
public static class RecognitionDigestWorkflow
{
    public static RecognitionDigestManager NewRecognitionDigestManager(IRecognitionDigestStore store, IClock clock)
    {
        return new RecognitionDigestManager(
            store,
            RecognitionDigestRules.DefaultRecognitionDigestValidator(),
            clock,
            new List<string>());
    }
}
