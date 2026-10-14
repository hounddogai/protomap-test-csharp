namespace Acme.Workforce.Engagement.Recognition.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition schedules through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionScheduleWorkflow(IRecognitionScheduleStore Store, RecognitionScheduleValidator Validator, IClock Clock, List<string> Journal, RecognitionScheduleEngine Engine)
{
    public static RecognitionScheduleWorkflow NewRecognitionScheduleWorkflow(IRecognitionScheduleStore store, IClock clock, Registry flags)
    {
        return new RecognitionScheduleWorkflow(
            store,
            RecognitionScheduleRules.DefaultRecognitionScheduleValidator(),
            clock,
            new List<string>(),
            RecognitionScheduleEngine.DefaultRecognitionScheduleEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition schedule {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionScheduleStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition schedule {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionScheduleStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition schedule {code} not found");
        }
        var result = Engine.Run(item);
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
