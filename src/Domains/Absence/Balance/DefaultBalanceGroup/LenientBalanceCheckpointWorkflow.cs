namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient balance checkpoints through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record LenientBalanceCheckpointWorkflow(ILenientBalanceCheckpointStore Store, LenientBalanceCheckpointValidator Validator, IClock Clock, List<string> Journal)
{
    public static LenientBalanceCheckpointWorkflow NewLenientBalanceCheckpointWorkflow(ILenientBalanceCheckpointStore store, IClock clock)
    {
        return new LenientBalanceCheckpointWorkflow(
            store,
            LenientBalanceCheckpointRules.DefaultLenientBalanceCheckpointValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance checkpoint {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientBalanceCheckpointStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient balance checkpoint {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientBalanceCheckpointStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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
