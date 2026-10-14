namespace Acme.Workforce.Engagement.Recognition.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves recognition plans through their life cycle and keeps a journal of what it did.</summary>
public sealed record RecognitionPlanManager(IRecognitionPlanStore Store, RecognitionPlanValidator Validator, IClock Clock, List<string> Journal, RecognitionPlanEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition plan {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RecognitionPlanStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition plan {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RecognitionPlanStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"recognition plan {code} not found");
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

/// <summary>Functions of recognition plan workflow.</summary>
public static class RecognitionPlanWorkflow
{
    public static RecognitionPlanManager NewRecognitionPlanManager(IRecognitionPlanStore store, IClock clock, Registry flags)
    {
        return new RecognitionPlanManager(
            store,
            RecognitionPlanRules.DefaultRecognitionPlanValidator(),
            clock,
            new List<string>(),
            RecognitionPlanEngine.DefaultRecognitionPlanEngine(flags));
    }
}
