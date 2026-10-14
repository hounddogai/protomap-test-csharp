namespace Acme.Workforce.Absence.Holiday.HolidayPolicy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves regional holiday policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalHolidayPolicyCoordinator(IRegionalHolidayPolicyStore Store, RegionalHolidayPolicyValidator Validator, IClock Clock, List<string> Journal, RegionalHolidayPolicyEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalHolidayPolicyStatus.Scheduled);
        Store.Save(item);
        Note($"{code} is scheduled");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalHolidayPolicyStatus.Cancelled);
        Store.Save(item);
        Note($"{code} is cancelled: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional holiday policy {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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

/// <summary>Functions of regional holiday policy workflow.</summary>
public static class RegionalHolidayPolicyWorkflow
{
    public static RegionalHolidayPolicyCoordinator NewRegionalHolidayPolicyCoordinator(IRegionalHolidayPolicyStore store, IClock clock, Registry flags)
    {
        return new RegionalHolidayPolicyCoordinator(
            store,
            RegionalHolidayPolicyRules.DefaultRegionalHolidayPolicyValidator(),
            clock,
            new List<string>(),
            RegionalHolidayPolicyEngine.DefaultRegionalHolidayPolicyEngine(flags));
    }
}
