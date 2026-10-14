namespace Acme.Workforce.Assets.Assignment.Baseline;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves lenient assignment scores through their life cycle and keeps a journal of what it did.</summary>
public sealed record LenientAssignmentScoreDesk(ILenientAssignmentScoreStore Store, LenientAssignmentScoreValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient assignment score {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(LenientAssignmentScoreStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"lenient assignment score {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(LenientAssignmentScoreStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
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

/// <summary>Functions of lenient assignment score workflow.</summary>
public static class LenientAssignmentScoreWorkflow
{
    public static LenientAssignmentScoreDesk NewLenientAssignmentScoreDesk(ILenientAssignmentScoreStore store, IClock clock)
    {
        return new LenientAssignmentScoreDesk(
            store,
            LenientAssignmentScoreRules.DefaultLenientAssignmentScoreValidator(),
            clock,
            new List<string>());
    }
}
