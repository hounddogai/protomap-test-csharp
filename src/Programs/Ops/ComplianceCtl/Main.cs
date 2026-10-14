namespace Acme.Workforce.Ops.ComplianceCtl;

using Acme.Workforce.Compliance.TrainingDue.Variance;
using Acme.Workforce.Kit.Clock;

public static class Program
{
    public static void Main()
    {
        var clock = Clock.System();
        var commands = new Dictionary<string, Func<bool>>
        {
            ["training-due-target"] = () =>
            {
                var store = TrainingDueTargetStore.NewMemoryTrainingDueTargetStore();
                for (var index = 0; index < 7; index++)
                {
                    store.Save(TrainingDueTarget.NewTrainingDueTarget($"training-due-target-{index}", $"Sample training due target {index}"));
                }
                var flow = TrainingDueTargetWorkflow.NewTrainingDueTargetWorkflow(store, clock);
                try
                {
                    flow.Submit("training-due-target-0");
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
                var items = store.List();
                var report = TrainingDueTargetReport.BuildTrainingDueTargetReport("Training Due Target", items, clock);
                Console.WriteLine(report.Render());
                return flow.Sweep((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()) >= 0;
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
