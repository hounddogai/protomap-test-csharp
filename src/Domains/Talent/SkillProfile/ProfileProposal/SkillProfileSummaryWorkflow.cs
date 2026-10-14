namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves skill profile summaries through their life cycle and keeps a journal of what it did.</summary>
public sealed record SkillProfileSummaryManager(ISkillProfileSummaryStore Store, SkillProfileSummaryValidator Validator, IClock Clock, List<string> Journal, SkillProfileSummaryEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile summary {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SkillProfileSummaryStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile summary {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SkillProfileSummaryStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public double Evaluate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"skill profile summary {code} not found");
        }
        var result = Engine.Price(item);
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

/// <summary>Functions of skill profile summary workflow.</summary>
public static class SkillProfileSummaryWorkflow
{
    public static SkillProfileSummaryManager NewSkillProfileSummaryManager(ISkillProfileSummaryStore store, IClock clock, Registry flags)
    {
        return new SkillProfileSummaryManager(
            store,
            SkillProfileSummaryRules.DefaultSkillProfileSummaryValidator(),
            clock,
            new List<string>(),
            SkillProfileSummaryEngine.DefaultSkillProfileSummaryEngine(flags));
    }
}
