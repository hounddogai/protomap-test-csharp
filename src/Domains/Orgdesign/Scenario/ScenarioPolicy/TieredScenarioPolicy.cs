namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered scenario policy is in its life cycle.</summary>
public enum TieredScenarioPolicyStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A tiered scenario policy as the scenario team keeps it.</summary>
public sealed class TieredScenarioPolicy(string code, string title, double rate, int startsAt, bool pinned, double cost, TieredScenarioPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int StartsAt { get; set; } = startsAt;
    public bool Pinned { get; set; } = pinned;
    public double Cost { get; set; } = cost;
    public TieredScenarioPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a tiered scenario policy in status new.</summary>
    public static TieredScenarioPolicy NewTieredScenarioPolicy(string code, string title)
    {
        return new TieredScenarioPolicy(code, title, 0.0, 0, false, 0.0, TieredScenarioPolicyStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredScenarioPolicyStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredScenarioPolicyStatus next)
    {
        switch (Status)
        {
            case TieredScenarioPolicyStatus.New:
                return next == TieredScenarioPolicyStatus.Queued || next == TieredScenarioPolicyStatus.Failed;
            case TieredScenarioPolicyStatus.Queued:
                switch (next)
                {
                    case TieredScenarioPolicyStatus.Done:
                        return true;
                }
                return false;
            case TieredScenarioPolicyStatus.Running:
                switch (next)
                {
                    case TieredScenarioPolicyStatus.Done:
                        return true;
                }
                return false;
            case TieredScenarioPolicyStatus.Done:
                switch (next)
                {
                    case TieredScenarioPolicyStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredScenarioPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered scenario policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered scenario policy"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)StartsAt + Cost, 2);
    }

    public TieredScenarioPolicy Copy()
    {
        return new TieredScenarioPolicy(Code, Title, Rate, StartsAt, Pinned, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
