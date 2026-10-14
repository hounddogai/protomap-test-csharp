namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy training due batches through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyTrainingDueBatchWorkflow(ILegacyTrainingDueBatchStore Store, LegacyTrainingDueBatchValidator Validator, IClock Clock, List<string> Journal, LegacyTrainingDueBatchEngine Engine)
{
    public static LegacyTrainingDueBatchWorkflow NewLegacyTrainingDueBatchWorkflow(ILegacyTrainingDueBatchStore store, IClock clock, Registry flags)
    {
        return new LegacyTrainingDueBatchWorkflow(
            store,
            LegacyTrainingDueBatchRules.DefaultLegacyTrainingDueBatchValidator(),
            clock,
            new List<string>(),
            LegacyTrainingDueBatchEngine.DefaultLegacyTrainingDueBatchEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due batch {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTrainingDueBatchStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due batch {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTrainingDueBatchStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due batch {code} not found");
        }
        var result = Engine.Evaluate(item);
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
