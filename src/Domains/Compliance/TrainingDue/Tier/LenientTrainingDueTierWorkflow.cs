namespace Acme.Workforce.Compliance.TrainingDue.Tier;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient training due tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientTrainingDueTierWorkflow(ILenientTrainingDueTierStore Store, LenientTrainingDueTierValidator Validator, IClock Clock, List<string> Journal, LenientTrainingDueTierEngine Engine)
{
    public static LenientTrainingDueTierWorkflow NewLenientTrainingDueTierWorkflow(ILenientTrainingDueTierStore store, IClock clock)
    {
        return new LenientTrainingDueTierWorkflow(
            store,
            LenientTrainingDueTierRules.DefaultLenientTrainingDueTierValidator(),
            clock,
            new List<string>(),
            LenientTrainingDueTierEngine.DefaultLenientTrainingDueTierEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient training due tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientTrainingDueTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient training due tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientTrainingDueTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient training due tier {code} not found");
        }
        var result = Engine.Evaluate(item);
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
