namespace Acme.Workforce.Compliance.TrainingDue.Assignment;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves training due policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record TrainingDuePolicyDesk(ITrainingDuePolicyStore Store, TrainingDuePolicyValidator Validator, IClock Clock, List<string> Journal, TrainingDuePolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TrainingDuePolicyStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TrainingDuePolicyStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"training due policy {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of training due policy workflow.</summary>
public static class TrainingDuePolicyWorkflow
{
    public static TrainingDuePolicyDesk NewTrainingDuePolicyDesk(ITrainingDuePolicyStore store, IClock clock)
    {
        return new TrainingDuePolicyDesk(
            store,
            TrainingDuePolicyRules.DefaultTrainingDuePolicyValidator(),
            clock,
            new List<string>(),
            TrainingDuePolicyEngine.DefaultTrainingDuePolicyEngine());
    }
}
