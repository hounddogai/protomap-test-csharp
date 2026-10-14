namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves tiered skill profile summaries through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record TieredSkillProfileSummaryService(ITieredSkillProfileSummaryStore Store, TieredSkillProfileSummaryValidator Validator, IClock Clock, List<string> Journal, TieredSkillProfileSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered skill profile summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredSkillProfileSummaryStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered skill profile summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredSkillProfileSummaryStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered skill profile summary {code} not found");
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

/// <summary>Functions of tiered skill profile summary workflow.</summary>
public static class TieredSkillProfileSummaryWorkflow
{
    public static TieredSkillProfileSummaryService NewTieredSkillProfileSummaryService(ITieredSkillProfileSummaryStore store, IClock clock, Registry flags)
    {
        return new TieredSkillProfileSummaryService(
            store,
            TieredSkillProfileSummaryRules.DefaultTieredSkillProfileSummaryValidator(),
            clock,
            new List<string>(),
            TieredSkillProfileSummaryEngine.DefaultTieredSkillProfileSummaryEngine(flags));
    }
}
