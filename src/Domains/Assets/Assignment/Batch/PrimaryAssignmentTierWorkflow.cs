namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary assignment tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryAssignmentTierWorkflow(IPrimaryAssignmentTierStore Store, PrimaryAssignmentTierValidator Validator, IClock Clock, List<string> Journal, PrimaryAssignmentTierEngine Engine)
{
    public static PrimaryAssignmentTierWorkflow NewPrimaryAssignmentTierWorkflow(IPrimaryAssignmentTierStore store, IClock clock, Registry flags)
    {
        return new PrimaryAssignmentTierWorkflow(
            store,
            PrimaryAssignmentTierRules.DefaultPrimaryAssignmentTierValidator(),
            clock,
            new List<string>(),
            PrimaryAssignmentTierEngine.DefaultPrimaryAssignmentTierEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary assignment tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryAssignmentTierStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary assignment tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryAssignmentTierStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary assignment tier {code} not found");
        }
        var result = Engine.Compute(item);
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
