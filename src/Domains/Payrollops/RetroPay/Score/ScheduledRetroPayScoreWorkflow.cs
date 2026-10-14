namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves scheduled retro pay scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record ScheduledRetroPayScoreWorkflow(IScheduledRetroPayScoreStore Store, ScheduledRetroPayScoreValidator Validator, IClock Clock, List<string> Journal, ScheduledRetroPayScoreEngine Engine)
{
    public static ScheduledRetroPayScoreWorkflow NewScheduledRetroPayScoreWorkflow(IScheduledRetroPayScoreStore store, IClock clock)
    {
        return new ScheduledRetroPayScoreWorkflow(
            store,
            ScheduledRetroPayScoreRules.DefaultScheduledRetroPayScoreValidator(),
            clock,
            new List<string>(),
            ScheduledRetroPayScoreEngine.DefaultScheduledRetroPayScoreEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled retro pay score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ScheduledRetroPayScoreStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled retro pay score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ScheduledRetroPayScoreStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"scheduled retro pay score {code} not found");
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
