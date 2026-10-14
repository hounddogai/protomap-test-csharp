namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary headcount agreements through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record PrimaryHeadcountAgreementWorkflow(IPrimaryHeadcountAgreementStore Store, PrimaryHeadcountAgreementValidator Validator, IClock Clock, List<string> Journal, PrimaryHeadcountAgreementEngine Engine)
{
    public static PrimaryHeadcountAgreementWorkflow NewPrimaryHeadcountAgreementWorkflow(IPrimaryHeadcountAgreementStore store, IClock clock, Registry flags)
    {
        return new PrimaryHeadcountAgreementWorkflow(
            store,
            PrimaryHeadcountAgreementRules.DefaultPrimaryHeadcountAgreementValidator(),
            clock,
            new List<string>(),
            PrimaryHeadcountAgreementEngine.DefaultPrimaryHeadcountAgreementEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount agreement {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryHeadcountAgreementStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount agreement {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryHeadcountAgreementStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary headcount agreement {code} not found");
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
            if (item.CreatedAt > cutoff)
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
