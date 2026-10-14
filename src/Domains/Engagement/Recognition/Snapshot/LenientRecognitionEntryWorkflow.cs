namespace Acme.Workforce.Engagement.Recognition.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves lenient recognition entries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientRecognitionEntryCoordinator(ILenientRecognitionEntryStore Store, LenientRecognitionEntryValidator Validator, IClock Clock, List<string> Journal, LenientRecognitionEntryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient recognition entry {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientRecognitionEntryStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient recognition entry {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientRecognitionEntryStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient recognition entry {code} not found");
        }
        var result = Engine.Compute(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
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

/// <summary>Functions of lenient recognition entry workflow.</summary>
public static class LenientRecognitionEntryWorkflow
{
    public static LenientRecognitionEntryCoordinator NewLenientRecognitionEntryCoordinator(ILenientRecognitionEntryStore store, IClock clock, Registry flags)
    {
        return new LenientRecognitionEntryCoordinator(
            store,
            LenientRecognitionEntryRules.DefaultLenientRecognitionEntryValidator(),
            clock,
            new List<string>(),
            LenientRecognitionEntryEngine.DefaultLenientRecognitionEntryEngine(flags));
    }
}
