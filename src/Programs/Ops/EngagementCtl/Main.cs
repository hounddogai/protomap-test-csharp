namespace Acme.Workforce.Ops.EngagementCtl;

using Acme.Workforce.Engagement.ActionPlan.Entry;
using Acme.Workforce.Engagement.Feedback.Preference;
using Acme.Workforce.Engagement.PulseSurvey.Claim;
using Acme.Workforce.Engagement.Recognition.Request;
using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["pulse-survey-claim"] = () =>
            {
                var store = PulseSurveyClaimStore.NewMemoryPulseSurveyClaimStore();
                for (var index = 0; index < 5; index++)
                {
                    store.Save(PulseSurveyClaim.NewPulseSurveyClaim($"pulse-survey-claim-{index}", $"Sample pulse survey claim {index}"));
                }
                var flow = PulseSurveyClaimWorkflow.NewPulseSurveyClaimService(store, clock);
                try
                {
                    flow.Advance("pulse-survey-claim-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = PulseSurveyClaimReport.BuildPulseSurveyClaimReport("Pulse Survey Claim", items, clock);
                Console.WriteLine(report.Render());
                return flow.Scan((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["action-plan-entry"] = () =>
            {
                var store = ActionPlanEntryStore.NewMemoryActionPlanEntryStore();
                for (var index = 0; index < 3; index++)
                {
                    store.Save(ActionPlanEntry.NewActionPlanEntry($"action-plan-entry-{index}", $"Sample action plan entry {index}"));
                }
                var flow = ActionPlanEntryWorkflow.NewActionPlanEntryDesk(store, clock);
                try
                {
                    flow.Start("action-plan-entry-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = ActionPlanEntryReport.BuildActionPlanEntryReport("Action Plan Entry", items, clock);
                Console.WriteLine(report.Render());
                return flow.Audit((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["recognition-reservation"] = () =>
            {
                var store = RecognitionReservationStore.NewMemoryRecognitionReservationStore();
                for (var index = 0; index < 4; index++)
                {
                    store.Save(RecognitionReservation.NewRecognitionReservation($"recognition-reservation-{index}", $"Sample recognition reservation {index}"));
                }
                var flow = RecognitionReservationWorkflow.NewRecognitionReservationCoordinator(store, clock, Flags.NewRegistry());
                try
                {
                    flow.Start("recognition-reservation-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = RecognitionReservationReport.BuildRecognitionReservationReport("Recognition Reservation", items, clock);
                Console.WriteLine(report.Render());
                return flow.Sweep((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
            },
            ["feedback-schedule"] = () =>
            {
                var store = FeedbackScheduleStore.NewMemoryFeedbackScheduleStore();
                for (var index = 0; index < 4; index++)
                {
                    store.Save(FeedbackSchedule.NewFeedbackSchedule($"feedback-schedule-{index}", $"Sample feedback schedule {index}"));
                }
                var flow = FeedbackScheduleWorkflow.NewFeedbackScheduleWorkflow(store, clock);
                try
                {
                    flow.Advance("feedback-schedule-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = FeedbackScheduleReport.BuildFeedbackScheduleReport("Feedback Schedule", items, clock);
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
