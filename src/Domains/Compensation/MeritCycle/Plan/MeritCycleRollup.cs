namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle rollup is in its life cycle.</summary>
public enum MeritCycleRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a merit cycle rollup and its current status.</summary>
public sealed class MeritCycleRollup(string code, string title, Dictionary<string, string> attributes, double fte, string location, string currency, string notes, string channel, MeritCycleRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Fte { get; set; } = fte;
    public string Location { get; set; } = location;
    public string Currency { get; set; } = currency;
    public string Notes { get; set; } = notes;
    public string Channel { get; set; } = channel;
    public MeritCycleRollupStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle rollup in status new.</summary>
    public static MeritCycleRollup NewMeritCycleRollup(string code, string title)
    {
        return new MeritCycleRollup(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            "",
            "",
            "",
            "",
            MeritCycleRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleRollupStatus next)
    {
        switch (Status)
        {
            case MeritCycleRollupStatus.New:
                switch (next)
                {
                    case MeritCycleRollupStatus.Queued:
                    case MeritCycleRollupStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleRollupStatus.Queued:
                return next == MeritCycleRollupStatus.Done;
            case MeritCycleRollupStatus.Running:
                return next == MeritCycleRollupStatus.Done || next == MeritCycleRollupStatus.Failed;
            case MeritCycleRollupStatus.Done:
                switch (next)
                {
                    case MeritCycleRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"merit cycle rollup"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public MeritCycleRollup Copy()
    {
        return new MeritCycleRollup(Code, Title, Attributes, Fte, Location, Currency, Notes, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
