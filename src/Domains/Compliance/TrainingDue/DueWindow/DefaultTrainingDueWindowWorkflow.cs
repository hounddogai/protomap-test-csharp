namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves default training due windows through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record DefaultTrainingDueWindowManager(IDefaultTrainingDueWindowStore Store, DefaultTrainingDueWindowValidator Validator, IClock Clock, List<string> Journal, DefaultTrainingDueWindowEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due window {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(DefaultTrainingDueWindowStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due window {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(DefaultTrainingDueWindowStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"default training due window {code} not found");
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

/// <summary>Functions of default training due window workflow.</summary>
public static class DefaultTrainingDueWindowWorkflow
{
    public static DefaultTrainingDueWindowManager NewDefaultTrainingDueWindowManager(IDefaultTrainingDueWindowStore store, IClock clock)
    {
        return new DefaultTrainingDueWindowManager(
            store,
            DefaultTrainingDueWindowRules.DefaultDefaultTrainingDueWindowValidator(),
            clock,
            new List<string>(),
            DefaultTrainingDueWindowEngine.DefaultDefaultTrainingDueWindowEngine());
    }
}
