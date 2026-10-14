namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves sentiment records through their life cycle and keeps a journal of what it did.</summary>
public sealed record SentimentRecordManager(ISentimentRecordStore Store, SentimentRecordValidator Validator, IClock Clock, List<string> Journal, SentimentRecordEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment record {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SentimentRecordStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment record {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SentimentRecordStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"sentiment record {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
            if (item.UpdatedAt > cutoff)
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

/// <summary>Functions of sentiment record workflow.</summary>
public static class SentimentRecordWorkflow
{
    public static SentimentRecordManager NewSentimentRecordManager(ISentimentRecordStore store, IClock clock, Registry flags)
    {
        return new SentimentRecordManager(
            store,
            SentimentRecordRules.DefaultSentimentRecordValidator(),
            clock,
            new List<string>(),
            SentimentRecordEngine.DefaultSentimentRecordEngine(flags));
    }
}
