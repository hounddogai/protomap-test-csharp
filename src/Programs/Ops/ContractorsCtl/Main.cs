namespace Acme.Workforce.Ops.ContractorsCtl;

using Acme.Workforce.Contractors.Rate.RecordItem;
using Acme.Workforce.Contractors.StatementOfWork.Assignment;
using Acme.Workforce.Contractors.Timesheet.Reservation;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["statement-of-work-agreement"] = () =>
            {
                var store = StatementOfWorkAgreementStore.NewMemoryStatementOfWorkAgreementStore();
                for (var index = 0; index < 3; index++)
                {
                    store.Save(StatementOfWorkAgreement.NewStatementOfWorkAgreement($"statement-of-work-agreement-{index}", $"Sample statement of work agreement {index}"));
                }
                var flow = StatementOfWorkAgreementWorkflow.NewStatementOfWorkAgreementCoordinator(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Open("statement-of-work-agreement-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = StatementOfWorkAgreementReport.BuildStatementOfWorkAgreementReport("Statement Of Work Agreement", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["timesheet-reservation"] = () =>
            {
                var store = TimesheetReservationStore.NewMemoryTimesheetReservationStore();
                for (var index = 0; index < 7; index++)
                {
                    store.Save(TimesheetReservation.NewTimesheetReservation($"timesheet-reservation-{index}", $"Sample timesheet reservation {index}"));
                }
                var flow = TimesheetReservationWorkflow.NewTimesheetReservationService(store, clock);
                try
                {
                    flow.Open("timesheet-reservation-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = TimesheetReservationReport.BuildTimesheetReservationReport("Timesheet Reservation", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["rate-proposal"] = () =>
            {
                var store = RateProposalStore.NewMemoryRateProposalStore();
                for (var index = 0; index < 6; index++)
                {
                    store.Save(RateProposal.NewRateProposal($"rate-proposal-{index}", $"Sample rate proposal {index}"));
                }
                var flow = RateProposalWorkflow.NewRateProposalWorkflow(store, clock);
                try
                {
                    flow.Advance("rate-proposal-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = RateProposalReport.BuildRateProposalReport("Rate Proposal", items, clock);
                Console.WriteLine(report.Render());
                return flow.ReviewStale((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
        };
        var failed = 0;
        foreach (var name in commands.Keys.Order(StringComparer.Ordinal))
        {
            var command = commands[name];
            if (!command())
            {
                Console.WriteLine($"{name} failed");
                failed += 1;
            }
        }
        if (failed > 0)
        {
            Environment.Exit(1);
        }
    }
}
