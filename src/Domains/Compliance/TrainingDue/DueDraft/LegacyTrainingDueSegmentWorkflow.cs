namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves legacy training due segments through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LegacyTrainingDueSegmentDesk(ILegacyTrainingDueSegmentStore Store, LegacyTrainingDueSegmentValidator Validator, IClock Clock, List<string> Journal, LegacyTrainingDueSegmentEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due segment {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LegacyTrainingDueSegmentStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due segment {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LegacyTrainingDueSegmentStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"legacy training due segment {code} not found");
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

/// <summary>Functions of legacy training due segment workflow.</summary>
public static class LegacyTrainingDueSegmentWorkflow
{
    public static LegacyTrainingDueSegmentDesk NewLegacyTrainingDueSegmentDesk(ILegacyTrainingDueSegmentStore store, IClock clock, Registry flags)
    {
        return new LegacyTrainingDueSegmentDesk(
            store,
            LegacyTrainingDueSegmentRules.DefaultLegacyTrainingDueSegmentValidator(),
            clock,
            new List<string>(),
            LegacyTrainingDueSegmentEngine.DefaultLegacyTrainingDueSegmentEngine(flags));
    }
}
