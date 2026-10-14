namespace Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves primary payslip snapshots through their life cycle and keeps a journal of what it did.</summary>
public sealed record PrimaryPayslipSnapshotWorkflow(IPrimaryPayslipSnapshotStore Store, PrimaryPayslipSnapshotValidator Validator, IClock Clock, List<string> Journal, PrimaryPayslipSnapshotEngine Engine)
{
    public static PrimaryPayslipSnapshotWorkflow NewPrimaryPayslipSnapshotWorkflow(IPrimaryPayslipSnapshotStore store, IClock clock, Registry flags)
    {
        return new PrimaryPayslipSnapshotWorkflow(
            store,
            PrimaryPayslipSnapshotRules.DefaultPrimaryPayslipSnapshotValidator(),
            clock,
            new List<string>(),
            PrimaryPayslipSnapshotEngine.DefaultPrimaryPayslipSnapshotEngine(flags));
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary payslip snapshot {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PrimaryPayslipSnapshotStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary payslip snapshot {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PrimaryPayslipSnapshotStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public double Estimate(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"primary payslip snapshot {code} not found");
        }
        var result = Engine.Run(item);
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
